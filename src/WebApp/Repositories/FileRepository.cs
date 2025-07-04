using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Repositories;

public class FileRepository
{
    private readonly AppDbContext _context;

    public FileRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<TicketFile?> GetFileByIdAsync(int fileId)
    {
        return await _context.TicketFiles
            .FirstOrDefaultAsync(f => f.Id == fileId);
    }
    public async Task AddFileAsync(TicketFile file)
    {
        await _context.TicketFiles.AddAsync(file);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}