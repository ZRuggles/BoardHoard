using BoardHoardApi.Data;
using Boardgames.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class BoardGamesController(GamesDbContext db) : ControllerBase
{
    private void EnsureImageUrl(IGame g)
    {
        if (g.ImageId is int id && string.IsNullOrWhiteSpace(g.ImageUrl))
            g.ImageUrl = $"{Request.Scheme}://{Request.Host}/api/images/{id}";
    }

    private string? ResolveUserId()
    {
        if (Request.Headers.TryGetValue("X-UserId", out var hdr) && !string.IsNullOrWhiteSpace(hdr))
            return hdr.ToString();
        return null;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BoardGame>>> GetAll()
    {
        var list = await db.BoardGames.OrderBy(g => g.Title).ToListAsync();
        foreach (var g in list) EnsureImageUrl(g);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BoardGame>> GetById(int id)
    {
        var g = await db.BoardGames.FindAsync(id);
        if (g is null) return NotFound();
        EnsureImageUrl(g);
        return Ok(g);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<BoardGame>> Create(BoardGame game)
    {
        var uid = ResolveUserId();
        if (uid is null) return Unauthorized();
        game.CreatedByUserId = uid;
        EnsureImageUrl(game);
        db.BoardGames.Add(game);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BoardGame game)
    {
        if (id != game.Id) return BadRequest();
        var exists = await db.BoardGames.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();
        EnsureImageUrl(game);
        db.Entry(game).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await db.BoardGames.FindAsync(id);
        if (g is null) return NotFound();

        // Also delete associated image if present
        if (g.ImageId is int imageId)
        {
            var img = await db.Images.FindAsync(imageId);
            if (img is not null)
            {
                db.Images.Remove(img);
            }
        }
        db.BoardGames.Remove(g);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
