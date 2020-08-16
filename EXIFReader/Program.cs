//WunderVision 2020
//Main Entry Point, pass in a file, get a dump of all Exif Tags it can find

using System;
using System.Text;
using System.Threading.Tasks;
using EXIFReader.JFIF;
namespace EXIFReader
{
    class Program
    {
        static async Task<string> ParseFile(string path)
        {
            var file = await JPEGEXIFFile.Parse(path);
            if(file == null)
            {
                return "\nAn issue occurred while parsing the input file: " + path;
            }
            StringBuilder exifdata = new StringBuilder(1024);
            if (file.Exif != null)
            {
                foreach (var key in file.Exif.Tags.Keys)
                {
                    exifdata.Append($"{key.ToString()}: {file.Exif.GetTagString(key)}\n");
                }
            }
            else
            {
                exifdata.Append("No Exif Data Found");
            }
            return exifdata.ToString();
        }
        static void Main(string[] args)
        {
            Console.WriteLine("WunderVision .NetCore EXIF Dump");
            if (args.Length > 0)
            {
                string filePath = args[0];
                var awaiter = ParseFile(filePath);
                Console.Write("Parsing");
                while (!awaiter.IsCompleted)
                {
                    Console.Write(".");
                    awaiter.Wait(10);
                }
                Console.Write("\n");
                Console.WriteLine(awaiter.Result);
            }
            else
            {
                Console.WriteLine("Please include a file path");
            }            
        }
    }
}
