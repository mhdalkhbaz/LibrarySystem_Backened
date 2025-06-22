using LibrarySystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BookController : ControllerBase
    {

        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery][Required] int userId, [FromQuery] string? searchKey)
        {
            var books = await _bookService.GetBooksAsync(userId, searchKey);
            return Ok(books);
        }

        [HttpPost("{bookId}")]
        public async Task<IActionResult> BorrowOrReturnBook([FromRoute][Required] int bookId, [FromQuery][Required] bool isBorrowing, [FromQuery][Required] int userId)
        {
            await _bookService.BorrowOrReturnBookAsync(bookId, isBorrowing, userId);
            return Ok(StatusCodes.Status200OK);
        }
    }
}
