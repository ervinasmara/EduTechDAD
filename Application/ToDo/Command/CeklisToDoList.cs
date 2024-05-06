using Application.Core;
using MediatR;
using Persistence;

namespace Application.ToDo.Command;
public class CeklisToDoList
{
    public class Command : IRequest<Result<object>>
    {
        public Guid ToDoListId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<object>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari ToDoList Berdasarkan ID **/
            var toDoList = await _context.ToDoLists.FindAsync(request.ToDoListId);

            /** Langkah 2: Memeriksa Ketersediaan ToDoList **/
            if (toDoList == null)
                return Result<object>.Failure("ToDoList not found");

            /** Langkah 3: Mengubah Status ToDoList Menjadi Nonaktif **/
            toDoList.Status = 0;

            /** Langkah 4: Menyimpan Perubahan ke Database **/
            await _context.SaveChangesAsync(cancellationToken);

            /** Langkah 5: Mengembalikan Hasil dalam Bentuk Success Result dengan Pesan **/
            return Result<object>.Success(new { Message = "ToDoList status updated successfully" });
        }
    }
}