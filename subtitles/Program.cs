using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace subtitles
{
    class Program
    {
    
        static VerbType Verb = VerbType.None;
        static OutputType Output = OutputType.None;
        static SyncType Sync = SyncType.None;
        static ShiftType Shift = ShiftType.None;

        static List<Line> lines = new();

        static string? InputFileName;
        static string? OutputFileName;

        static string? SyncLine1;
        static string? SyncLine2;
        static string? SyncTimeStamp;

        static string? ShiftMilliseconds;
        static string? ShiftLine;
        static string? ShiftTimeStamp;
        static void Main(string[] args)
        {
            if (args.Length < 2) { PrintHelp(); return; }

            if (!CheckSourceFile(args[0])) { PrintHelp(); return; }
            Output = DetermineOutput(args);

            if(Output == OutputType.Invalid) { PrintHelp(); return; }

            Verb = DetermineVerb(args[1]);
            if (Verb == VerbType.None) { PrintHelp(); return; }
            if (Verb == VerbType.Utf)
            {
                Utf.Convert(InputFileName, OutputFileName);
            }
            if(Verb == VerbType.Sync || Verb == VerbType.Shift)
            {
                

                if (Verb == VerbType.Sync)
                {
                    Sync = DetermineSync(args);
                    if (Sync == SyncType.InValid)
                    {
                        PrintHelp();
                        return;
                    }
                    
                    lines.ReadFile(InputFileName);

                    int startLine = int.Parse(SyncLine1);
                    int endLine = int.Parse(SyncLine2);
                    DateTime endTime = SyncTimeStamp.ToDateTime();
                    lines.Sync(startLine, endLine, endTime);
                    
                }
                if (Verb == VerbType.Shift)
                {
                    Shift = DetermineShift(args);
                    if (Shift == ShiftType.InValid)
                    {
                        PrintHelp();
                        return;
                    }

                    lines.ReadFile(InputFileName);

                    if (Shift == ShiftType.OnlyMilliseconds)
                    {
                        int value = int.Parse(ShiftMilliseconds);
                        lines.Shift(value);
                    }
                    else if(Shift == ShiftType.LineAndTimeStamp)
                    {
                        int lineNumber = int.Parse(ShiftLine);
                        DateTime correctTime = ShiftTimeStamp.ToDateTime();
                        try
                        {
                            lines.Shift(lineNumber, correctTime);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            return;
                        }
                    }
                }


                lines.WriteToFile(OutputFileName);

                if(Output == OutputType.OverwriteSource) { Console.WriteLine("Source file overwritten."); }
                else if (Output == OutputType.OverwriteDestination) { Console.WriteLine("Destination file overwritten."); }
#if DEBUG
                //Process.Start("notepad", output);
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
#endif

            }


            if (Output == OutputType.Invalid)
            {
                PrintHelp();
                return;
            }
            else if (Output == OutputType.OverwriteSource)
            {
                OutputFileName = InputFileName;
            }
            else if (Output == OutputType.Provided)
            {
                OutputFileName = args.FindValue("out");
            }




        }

        static void PrintHelp()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            string name = Path.GetFileName(codeBase).Split(".").First();


            Console.WriteLine("Usage:");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"    {name} inputFile <command> <value(s)> [out output_filename]");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Examples:  ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    {name} the_village.srt utf8 out fixed.srt");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("       converts encoding of source file from Arabic windows-1256 to utf-8");
            Console.WriteLine("       utf8 encoding is saved to [fixed.srt] file");
            Console.WriteLine();


            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    {name} the_village.srt shift 1300");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("       shift timing in each subtitles lines by adding 1300 milliseconds, and overwrite original file");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    {name} the_village.srt shift 5 00:02:24,415");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("       the 5th line in the subtitles file should be on screen at the timestamp 00:02:24,415.");
            Console.WriteLine("       all other lines are shifted according to the found time span difference");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    {name} the_village.srt shift -26000 out the_village_fixed.srt");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("       shift timing in each subtitles lines by subtracting 26000 milliseconds, and save to the_village_fixed.srt");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    {name} the_village.srt sync 5 643 01:14:23,015");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("       the 5th line is a reference time and is correct both in file and in movie");
            Console.WriteLine("       the 643rd line is a out of sync and should be at the timestamp 01:14:23,015");
            Console.WriteLine("       all other lines will get decrement/increment ratio based on the reference times");
            Console.WriteLine();
        }
        
        
        

        static SyncType DetermineSync(string[] args)
        {
            if (!args.FindKey("sync")) return SyncType.None;

            string[] parameters = args.Between("sync", "out");
            if (parameters.Length != 3)
            {
                Console.WriteLine($"varb [sync] is missing values");
                return SyncType.InValid;
            }


            if (parameters.Length == 3)
            {
                if (parameters[0].IsPositiveInteger() && parameters[1].IsPositiveInteger() && parameters[2].IsTimeStamp())
                {
                    SyncLine1 = parameters[0];
                    SyncLine2 = parameters[1];
                    SyncTimeStamp = parameters[2];
                    return SyncType.Valid;
                }
                Console.WriteLine($"varb [sync] should followed by a positive line number, positive line number, and a time stamp. line:[{parameters[0]}], line:[{parameters[1]}] and timestamp: [{parameters[2]}] are not valid");
                return SyncType.InValid;
            }
            return SyncType.InValid;
        }

        static ShiftType DetermineShift(string[] args)
        {
            if (!args.FindKey("shift")) return ShiftType.None;

            string[] parameters = args.Between("shift", "out");
            if (parameters.Length == 0)
            {
                Console.WriteLine($"varb [shift] is missing values");
                return ShiftType.InValid;
            }
            if (parameters.Length > 2) return ShiftType.InValid;
            if (parameters.Length == 1)
            {
                if (parameters[0].IsInteger())
                {
                    ShiftMilliseconds = parameters[0];
                    return ShiftType.OnlyMilliseconds;
                }
                Console.WriteLine($"varb [shift] should followed by number of milliseconds. {parameters[0]} is not valid");
                return ShiftType.InValid;
            }
            if (parameters.Length == 2)
            {
                if (parameters[0].IsPositiveInteger() && parameters[1].IsTimeStamp())
                {
                    ShiftLine = parameters[0];
                    ShiftTimeStamp = parameters[1];
                    return ShiftType.LineAndTimeStamp;
                }
                Console.WriteLine($"varb [shift] should followed by a positive line number and a time stamp. line: [{parameters[0]}] and timestamp: [{parameters[1]}] are not valid");
                return ShiftType.InValid;
            }
            return ShiftType.InValid;
        }



        static bool CheckSourceFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"input file [{filename}] does not exist.");
                return false;
            }
            InputFileName = filename;
            return true;
        }

        static OutputType DetermineOutput(string[] args)
        {
            bool keyExists = args.FindKey("out");
            if (!keyExists)
            {
                //Console.WriteLine("source file will be overwritten");
                OutputFileName = args[0];
                return OutputType.OverwriteSource;
            }

            string? value = args.FindValue("out");
            if (value == null)
            {
                Console.WriteLine("found [out] without [output_filename].");
                return OutputType.Invalid;
            }

            OutputFileName = value;
            if (File.Exists(value))
            {
                //Console.WriteLine($"output file [{value}] already exists and will be overwritten");
                return OutputType.OverwriteDestination;
            }

            return OutputType.Provided;

        }
        static VerbType DetermineVerb(string verb)
        {
            switch (verb)
            {
                case "shift": return VerbType.Shift;
                case "sync": return VerbType.Sync;
                case "utf8":
                case "utf": return VerbType.Utf;
                default:
                    Console.WriteLine($"the verb {verb} is not valid");
                    return VerbType.None;
            }
        }

        

    }
}

