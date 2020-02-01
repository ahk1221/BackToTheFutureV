namespace BackToTheFutureV
{
    public abstract class Disposable
    {
        public Disposable()
        {
            Main.disposableObjects.Add(this);
        }

        public abstract void Dispose();
    }
}
