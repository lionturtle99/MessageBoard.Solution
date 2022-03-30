using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessageBoard.Models;
using System;

namespace MessageBoard.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class MessagesController : ControllerBase
  {
    private readonly MessageBoardContext _db;

    public MessagesController(MessageBoardContext db)
    {
      _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> Get(string datePublished)
    {
      DateTime currentDate = DateTime.UtcNow;
      DateTime userDate = DateTime.Parse(datePublished);
      var query = _db.Messages.AsQueryable();
      if (userDate < currentDate)
      {
        query = _db.Messages.Where(m => m.DatePublished < userDate);
        return await query.ToListAsync();
      }
      else 
      {
        return await _db.Messages.ToListAsync();
      }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetMessage(int id)
    {
      var message = await _db.Messages.FindAsync(id);
      if(message == null)
      {
        return NotFound();
      }
      return message;
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Post(Message message)
    {
      _db.Messages.Add(message);
      await _db.SaveChangesAsync();

      return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Message message)
    {
      if(id != message.MessageId)
      {
        return BadRequest();
      }
      _db.Entry(message).State = EntityState.Modified;

      try
      {
        await _db.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!MessageExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }
      return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
      var message = await _db.Messages.FindAsync(id);
      if (message == null)
      {
        return NotFound();
      }

      _db.Messages.Remove(message);
      await _db.SaveChangesAsync();

      return NoContent();
    }

    private bool MessageExists(int id)
    {
      return _db.Messages.Any(m => m.MessageId == id);
    }
  }
}
