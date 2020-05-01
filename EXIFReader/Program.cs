using System;

using EXIFReader.JFIF;
namespace EXIFReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string filePath = "D:\\Documents\\ImageProjects\\Board.jpg";

            JFIFFile.Parse(filePath);

            Console.ReadKey();

            
        }
    }
}
