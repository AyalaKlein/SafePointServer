using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using FirebaseCloudMessaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SafePoint.Data;
using SafePoint.Data.Entities;

namespace SafePoint.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheltersController : ControllerBase
    {
        private readonly SafePointContext _context;

        public SheltersController(SafePointContext context)
        {
            _context = context;
        }

        // GET: api/Shelters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shelter>>> GetShelter()
        {
            return await _context.Shelters.ToListAsync();
        }

        // GET: api/Shelters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Shelter>> GetShelter(int id)
        {
            var shelter = await _context.Shelters.FindAsync(id);

            if (shelter == null)
            {
                return NotFound();
            }

            return shelter;
        }

        // PUT: api/Shelters/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShelter(int id, Shelter shelter)
        {
            if (id != shelter.Id)
            {
                return BadRequest();
            }

            _context.Entry(shelter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShelterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Shelters
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Shelter>> PostShelter(Shelter shelter)
        {
            shelter.Id = 0;
            _context.Shelters.Add(shelter);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetShelter", new { id = shelter.Id }, shelter);
        }

        // DELETE: api/Shelters/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Shelter>> DeleteShelter(int id)
        {
            var shelter = await _context.Shelters.FindAsync(id);
            if (shelter == null)
            {
                return NotFound();
            }

            _context.Shelters.Remove(shelter);
            await _context.SaveChangesAsync();

            return shelter;
        }

        private bool ShelterExists(int id)
        {
            return _context.Shelters.Any(e => e.Id == id);
        }

        [HttpGet("GetNearestShelters")]
        public async Task<ActionResult<NearestShelters>> GetNearestShelters(decimal locX, decimal locY, double meterRadius)
        {
            int refreshTime = 10 * 1000;
            var currentLocation = new Location(locX, locY);

            var allShelters = await _context.Shelters.ToListAsync();
            var shelters = allShelters.Where(currShelter => currentLocation.CalculateDistance(new Location(currShelter.LocX, currShelter.LocY)) > meterRadius).ToList();

            var result = new NearestShelters
            {
                refreshTime = refreshTime,
                shelters = shelters
            };

            return result;
        }

        [HttpGet("SearchForShelter")]
        public async Task<ActionResult<Shelter>> SearchForShelter(string operationGuid, string fcmToken, decimal locX, decimal locY)
        {
            const int avg_distance_meters = 450; // 450 meters for average person when fast walking

            // Remove the person from his old shelter
            var oldShelter = await _context.ShelterUsers.FirstAsync(x => x.UserToken == fcmToken);

            // Check if the user already changed shelter during this operation
            if (oldShelter.operationType == operationGuid)
            {
                return await _context.Shelters.FirstAsync(x => x.Id == oldShelter.ShelterId);
            }

            _context.ShelterUsers.Remove(oldShelter);
            await _context.SaveChangesAsync();

            var currentLocation = new Location(locX, locY);
            var closestShelters = (await GetNearestShelters(locX, locY, avg_distance_meters)).Value;

            double minDistance = double.MaxValue;
            double selectedShelterMinDistance = double.MaxValue;
            Shelter selectedShelter = null;
            Shelter closestShelter = null;

            closestShelters.shelters.ForEach(shel =>
            {
                var dist = currentLocation.CalculateDistance(new Location(shel.LocX, shel.LocY));

                // Get the closest available shelter
                if (dist < selectedShelterMinDistance && 
                    _context.ShelterUsers.CountAsync(x => x.ShelterId == shel.Id).Result < shel.MaxCapacity)
                {
                    selectedShelterMinDistance = dist;
                    selectedShelter = shel;
                }

                // Get the closest shelter
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestShelter = shel;
                }
            });

            Shelter chosenShelter = selectedShelter != null ? selectedShelter : closestShelter;

            List<string> tokens = await _context.ShelterUsers.Where(x => x.ShelterId == chosenShelter.Id).Select(x => x.UserToken).ToListAsync();

            _context.ShelterUsers.Add(new ShelterUsers()
            {
                ShelterId = chosenShelter.Id,
                UserToken = fcmToken,
                operationType = operationGuid
            });

            await _context.SaveChangesAsync();

            if (selectedShelter == null)
            {
                // Notify all the users in closestShelter.UsersInShelter to find new shelter using FCM and the opertion guid
                var message = new MulticastMessage()
                {
                    Data = new Dictionary<string, string>
                    {
                        ["operationGuid"] = operationGuid
                    },
                    Tokens = tokens
                };

                await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            }

            return chosenShelter;
        }
    }
}
