using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cli = new MvcMusicStore.Catalog.MusicStoreDBClient();

            var genre = cli.Genres();

            int a = 3;

        }
    }
}
