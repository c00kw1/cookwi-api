namespace Api.Hosting.Utils
{
    public static class Extensions
    {
        public static int Mb(this int nb)
        {
            return nb * 1_000_000;
        }
    }
}
