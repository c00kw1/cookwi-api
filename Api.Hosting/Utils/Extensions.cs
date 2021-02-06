namespace Api.Hosting.Utils
{
    public static class Extensions
    {
        public static int Mb(this int nb)
        {
            return nb * 1024 * 1024;
        }
    }
}
