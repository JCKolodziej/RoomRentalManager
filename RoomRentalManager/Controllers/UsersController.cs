﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomRentalManager.Models;

namespace RoomRentalManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RRM_DBContext _context;

        public UsersController(RRM_DBContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _context.User.ToListAsync();
        }

        // GET: api/Users/byId?id=1
        [HttpGet("byId")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/Users/byEmail?Email=a@a.a
        [HttpGet("byEmail")]
        public async Task<ActionResult<User>> GetUser(string Email)
        {
            var userList = await _context.User.Where(x => x.Email == Email).ToListAsync();

            if (userList.Count() == 0)
            {
                return NotFound();
            }

            return userList[0];
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();
            await Login.LoginUserAsync(user, HttpContext);

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // POST: api/Users/Login
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("Login")]
        public async Task<ActionResult<User>> LoginUser(User user)
        {
            string Email = user.Email;
            string Password = user.Password;
            var userList = await _context.User.Where(x => x.Email == Email).ToListAsync();

            if (userList.Count() == 0)
            {
                return NotFound();
            }
            else
            {
                User fullUser = userList[0];
                bool correctPassword = BCrypt.Net.BCrypt.Verify(Password, fullUser.Password);
                if (correctPassword)
                {
                    await Login.LoginUserAsync(fullUser, HttpContext);
                    return fullUser;
                }
                else
                {
                    return NotFound();
                }

            }

        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
