using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet] //get all auctions
    public async Task<List<AuctionDto>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
        .Include(a => a.Item)
        .OrderBy(a => a.Item.Make)
        .ToListAsync(); //get all auctions from the database and include the item

        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")] //get auction by id
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
        .Include(a => a.Item)
        .FirstOrDefaultAsync(a => a.Id == id); //get auction by id from the database and include the item

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost] //create auction
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto); //map the auctionDto to an auction

        _context.Auctions.Add(auction); //add the auction to the database
        // LATER : ADD CURRENT USER AS SELLER
        auction.Seller = "test";

        _context.Auctions.Add(auction); //add the auction to the database   


        var result = await _context.SaveChangesAsync() > 0; //save the changes to the database

        if (!result)
        {
            return BadRequest("Failed , Cannot save changes to the database");
        }

        return CreatedAtAction(nameof(GetAuctionById),
         new { id = auction.Id },
           _mapper.Map<AuctionDto>(auction)); //return the auctionDto
    }

    [HttpPut("{id}")] //update auction by id
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
        .Include(a => a.Item)
        .FirstOrDefaultAsync(a => a.Id == id); //get auction by id from the database and include the item

        if (auction == null)
        {
            return NotFound();
        }

        // LATER :  ceck if current user is the seller
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0; //save the changes to the database

        if (result) return Ok();

        return BadRequest("Failed , Cannot save changes to the database");
    }

    [HttpDelete("{id}")] //delete auction by id
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id); //get auction by id from the database

        if (auction == null)
        {
            return NotFound();
        }
        // LATER :  ceck if current user is the seller
        _context.Auctions.Remove(auction); //remove the auction from the database

        var result = await _context.SaveChangesAsync() > 0; //save the changes to the database

        if (!result)
        {
            return BadRequest("Failed , Cannot save changes to the database");
        }

        return Ok();
    }

}

