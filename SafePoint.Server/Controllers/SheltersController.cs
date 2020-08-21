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

        [HttpGet("SearchForShelter")]
        public async Task<ActionResult<Shelter>> SearchForShelter(string operationGuid, string fcmToken, decimal locX, decimal locY)
        {
            async Task AddUserToShelter(Shelter selectedShelter){
                _context.ShelterUsers.RemoveRange(_context.ShelterUsers.Where(o => o.UserToken == fcmToken));
                _context.ShelterUsers.Add(new ShelterUsers()
                {
                    ShelterId = selectedShelter.Id,
                    UserToken = fcmToken,
                    OperationType = operationGuid
                });

                await _context.SaveChangesAsync();
            }
            const int avg_distance_meters = 450; // 450 meters for average person when fast walking

            // Remove the person from his old shelter
            var oldShelter = await _context.ShelterUsers.Where(x => x.UserToken == fcmToken).ToListAsync();

            // Check if the user already changed shelter during this operation
            if (oldShelter != null && oldShelter.Count > 0)
            {
                if (oldShelter[0].OperationType == operationGuid)
                {
                    return await _context.Shelters.FirstAsync(x => x.Id == oldShelter[0].ShelterId);
                }

                _context.ShelterUsers.Remove(oldShelter[0]);
                await _context.SaveChangesAsync();
            }

            var currentLocation = new Location(locX, locY);
            const decimal pi180 = (decimal)(Math.PI / 180.0);
            var shelters = await _context.NearbyShelters.FromSqlInterpolated($@"
                SELECT * FROM
                (SELECT Id, MaxCapacity, LocX, LocY, UserToken, Distance = 6376500.0 * (2.0 * ATAN2(SQRT(Power(SIN(((LocY * {pi180}) - (LocX * {pi180}))/2), 2) + COS(LocX * {pi180}) * COS(LocY * {pi180}) *POW(SIN((LocX * {pi180} - (LocY-{pi180}))/2),2),
                SQRT(1 - SQRT(Power(SIN(((LocY * {pi180}) - (LocX * {pi180}))/2), 2) + COS(LocX * {pi180}) * COS(LocY * {pi180}) *POW(SIN((LocX * {pi180} - (LocY-{pi180}))/2),2))
                FROM Shelters
                   INNER JOIN SheltersUsers
                       on Shelters.Id = ShelterId)
                WHERE Distance < {avg_distance_meters}").ToListAsync();

            var result = shelters.GroupBy(g => new { g.Id, g.LocX, g.LocY, g.Distance, g.MaxCapacity }).Select(o => new ShelterInfo
            {
                Shelter = new Shelter
                {
                    Id = o.Key.Id,
                    LocX = o.Key.LocX,
                    LocY = o.Key.LocY,
                    MaxCapacity = o.Key.MaxCapacity,
                },
                Distance = o.Key.Distance,
                AssignedUsers = o.Select(o => o.UserToken).ToList(),
            }).OrderBy(o=> o.AssignedUsers.Count).ToList();


            ShelterInfo selectedShelter = null;

            foreach(var shelter in result)
            {
                if(shelter.AssignedUsers.Count < shelter.Shelter.MaxCapacity)
                {
                    selectedShelter = shelter;
                    break;
                }
            }
            if(selectedShelter != null)
            {
                await AddUserToShelter(selectedShelter.Shelter);
                return Ok(selectedShelter);
            }

            selectedShelter = result.OrderBy(o=> o.Distance).First();

            List<string> tokens = selectedShelter.AssignedUsers;

            await AddUserToShelter(selectedShelter.Shelter);

            // Notify all the users in closestShelter.UsersInShelter to find new shelter using FCM and the opertion guid
            var message = new MulticastMessage()
            {
                Data = new Dictionary<string, string>
                {
                    ["operationGuid"] = operationGuid
                },
                Tokens = tokens,
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            

            return Ok(selectedShelter);
        }

        [HttpPost("ChangeUserFcmToken")]
        public async void ChangeUserFcmToken(string oldToken, string newToken)
        {
            if (_context.ShelterUsers.Count(shel => shel.UserToken == oldToken) > 0)
            {
                var currShelterUser = await _context.ShelterUsers.FirstAsync(shel => shel.UserToken == oldToken);
                currShelterUser.UserToken = newToken;
                _context.ShelterUsers.Update(currShelterUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
