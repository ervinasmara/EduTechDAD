using Application.Core;
using MediatR;
using Persistence;

namespace Application.ToDo.Command;
public class DeleteToDoList
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid ToDoListId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari entitas ToDoList berdasarkan ID yang diberikan **/
            var toDoList = await _context.ToDoLists.FindAsync(request.ToDoListId);

            /** Langkah 2: Memeriksa apakah ToDoList ditemukan **/
            if (toDoList == null) return null;

            /** Langkah 3: Menghapus entitas ToDoList dari konteks database **/
            _context.Remove(toDoList);

            /** Langkah 4: Menyimpan perubahan ke database **/
            var result = await _context.SaveChangesAsync() > 0;

            /** Langkah 5: Memeriksa apakah penyimpanan berhasil **/
            if (!result) return Result<Unit>.Failure("Gagal untuk menghapus ToDoList");

            /** Langkah 6: Mengembalikan hasil berhasil tanpa data (Unit) **/
            return Result<Unit>.Success(Unit.Value);
        }
    }
}