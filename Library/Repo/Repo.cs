using Library.Model;

namespace Library.Repo
{
    public class Repo<T>: IRepo<T> where T : class
    {
        public LibraryContext Context { get; }
        public Repo(LibraryContext context)
        {
            Context = context;
        }


        public void Add(T item)
        {
            Context.Add(item);
        }

        public List<T> GetAll()
        {
            return (Context.Set<T>().ToList());
        }

        public T GetByFilter(Func<T, bool> Get)
        {
            return Context.Set<T>().Where(Get).FirstOrDefault();
        }

        public void AddRAnge(IEnumerable<T> item)
        {
            Context.Set<T>().AddRange(item);
        }
    }
}
