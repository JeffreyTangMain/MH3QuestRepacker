using System;
using System.IO;
using System.Text;

namespace MH3QuestRepacker
{
    class Program
    {
        static void Main(string[] args)
        {
            //string input = args[0];
            foreach (var arg in args)
            {
                string input = arg;
                if (Path.GetExtension(input) == ".txt")
                {

                    TxtInput(input);
                }
                else if (Path.GetExtension(input) == ".ustxt")
                {
                    USTxtInput(input);
                }
                else
                {
                    Console.WriteLine("Type 1 for JP files, type 2 for anything else");
                    string readinput = Console.ReadLine();
                    if(readinput.Equals("1"))
                    {
                        QuestInput(input);
                    } else
                    {
                        USQuestInput(input);
                    }
                }
            }
        }

        static void TxtInput(string input)
        {
            Encoding enc = Encoding.GetEncoding("shift-jis");
            StreamReader reader = new StreamReader(input, enc, true);
            string extension = reader.ReadLine();

            //string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".table";
            string headerDir = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".header";
            string gmdOutput = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".gmd";
            string originalFile = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + extension;
            string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + "2" + extension;
            //BinaryReader reader = new BinaryReader(File.OpenRead(input));

            BinaryReader binaryReader = new BinaryReader(File.OpenRead(originalFile));

            int fileLength = File.ReadAllBytes(originalFile).Length;
            int currentLength = 0;

            Console.WriteLine("INFO: output: " + output);

            if (File.Exists(output))
                File.Delete(output);

            using (FileStream fsStream = new FileStream(output, FileMode.Append))
            using (BinaryWriter writer = new BinaryWriter(fsStream, Encoding.UTF8))
            {
                //headerReader.Read(byteHeader, 0, 0x10); // 0x10 is hardcoded s_count_offset
                byte[] bytesRead = binaryReader.ReadBytes(8);
                //currentLength += 8;
                writer.Write(bytesRead);

                string line = reader.ReadLine();
                line = line.Replace("<LINE>", "\n");
                byte[] bytes = enc.GetBytes(line);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                binaryReader.ReadBytes(4 + bytes.Length);

                bytesRead = binaryReader.ReadBytes(2);
                writer.Write(bytesRead);

                line = reader.ReadLine();
                line = line.Replace("<LINE>", "\n");
                bytes = enc.GetBytes(line);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                binaryReader.ReadBytes(4 + bytes.Length);

                bytesRead = binaryReader.ReadBytes(9);
                writer.Write(bytesRead);

                line = reader.ReadLine();
                line = line.Replace("<LINE>", "\n");
                bytes = enc.GetBytes(line);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                binaryReader.ReadBytes(4 + bytes.Length);

                bytesRead = binaryReader.ReadBytes(6);
                writer.Write(bytesRead);

                line = reader.ReadLine();
                line = line.Replace("<LINE>", "\n");
                bytes = enc.GetBytes(line);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                binaryReader.ReadBytes(4 + bytes.Length);

                line = reader.ReadLine();
                line = line.Replace("<LINE>", "\n");
                bytes = enc.GetBytes(line);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                binaryReader.ReadBytes(4 + bytes.Length);

                var stream = binaryReader.BaseStream;
                int currentPos = (int)stream.Position;
                int remainingFile = fileLength - currentPos;

                bytesRead = binaryReader.ReadBytes(remainingFile);
                writer.Write(bytesRead);
            }

            reader.Close();
            binaryReader.Close();

            if (File.Exists(originalFile))
                File.Delete(originalFile);
            if (File.Exists(output))
                File.Move(output, originalFile);
        }

        static void USTxtInput(string input)
        {
            StreamReader reader = new StreamReader(input, Encoding.UTF8, true);
            //string extension = reader.ReadLine();

            //string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".table";
            string headerDir = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".header";
            string gmdOutput = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".gmd";
            string originalFile = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".quest";
            string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + "2.quest";
            //BinaryReader reader = new BinaryReader(File.OpenRead(input));

            BinaryReader binaryReader = new BinaryReader(File.OpenRead(originalFile));

            int fileLength = File.ReadAllBytes(originalFile).Length;
            int currentLength = 0;

            Console.WriteLine("INFO: output: " + output);

            if (File.Exists(output))
                File.Delete(output);

            using (FileStream fsStream = new FileStream(output, FileMode.Append))
            using (BinaryWriter writer = new BinaryWriter(fsStream, Encoding.UTF8))
            {
                //headerReader.Read(byteHeader, 0, 0x10); // 0x10 is hardcoded s_count_offset
                byte[] bytesRead = binaryReader.ReadBytes(8);
                //currentLength += 8;
                writer.Write(bytesRead);

                for (Int32 i = 0; i < 5; i++)
                {
                    string line = reader.ReadLine();
                    line = line.Replace("<LINE>", "\r\n");
                    byte[] bytes = Encoding.UTF8.GetBytes(line);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    binaryReader.ReadBytes(4 + bytes.Length);
                }

                bytesRead = binaryReader.ReadBytes(2);
                writer.Write(bytesRead);

                for (Int32 i = 0; i < 5; i++)
                {
                    string line = reader.ReadLine();
                    line = line.Replace("<LINE>", "\r\n");
                    byte[] bytes = Encoding.UTF8.GetBytes(line);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    binaryReader.ReadBytes(4 + bytes.Length);
                }

                bytesRead = binaryReader.ReadBytes(24);
                writer.Write(bytesRead);

                for (Int32 i = 0; i < 5; i++)
                {
                    string line = reader.ReadLine();
                    line = line.Replace("<LINE>", "\r\n");
                    byte[] bytes = Encoding.UTF8.GetBytes(line);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    binaryReader.ReadBytes(4 + bytes.Length);
                }

                bytesRead = binaryReader.ReadBytes(6);
                writer.Write(bytesRead);

                for (Int32 i = 0; i < 5; i++)
                {
                    string line = reader.ReadLine();
                    line = line.Replace("<LINE>", "\r\n");
                    byte[] bytes = Encoding.UTF8.GetBytes(line);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    binaryReader.ReadBytes(4 + bytes.Length);
                }

                for (Int32 i = 0; i < 5; i++)
                {
                    string line = reader.ReadLine();
                    line = line.Replace("<LINE>", "\r\n");
                    byte[] bytes = Encoding.UTF8.GetBytes(line);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    binaryReader.ReadBytes(4 + bytes.Length);
                }

                var stream = binaryReader.BaseStream;
                int currentPos = (int)stream.Position;
                int remainingFile = fileLength - currentPos;

                bytesRead = binaryReader.ReadBytes(remainingFile);
                writer.Write(bytesRead);
            }

            reader.Close();
            binaryReader.Close();

            if (File.Exists(originalFile))
                File.Delete(originalFile);
            if (File.Exists(output))
                File.Move(output, originalFile);
        }

        public static class fileOffsets
        {
            public static int s_count_offset = 0;
            public static int t_size_offset = 0;
        }

        public static void Pause()
        {
            Console.Write("Press any key to continue . . .");
            Console.ReadKey(true);
        }

        public static void offsetSetter(String file)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(file));
            reader.ReadInt32();
            int version = reader.ReadInt32();

            // Handle version offset differences
            if (version == 0x00010201 || version == 0x00010101) // MH3U EU, MH3G JP
            {
                fileOffsets.s_count_offset = 0x10;
                fileOffsets.t_size_offset = 0x18;
            }
            else if (version == 0x00010302 || version == 0x00020301) // MHX JP, MHXX
            {
                fileOffsets.s_count_offset = 0x18;
                fileOffsets.t_size_offset = 0x20;
            }
            else
            {
                Console.WriteLine("ERROR: Unsupported GM version, aborting.");
                return;
            }
            reader.Close();
        }

        static void QuestInput(string input)
        {
            string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".txt";
            BinaryReader reader = new BinaryReader(File.OpenRead(input));

            if (File.Exists(output))
                File.Delete(output);

            using (FileStream fsStream = new FileStream(output, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fsStream, Encoding.UTF8))
            {
                byte[] bytes = Encoding.ASCII.GetBytes(Path.GetExtension(input));
                byte[] newLine = { 0x0D, 0x0A };
                writer.Write(bytes);
                writer.Write(newLine);

                reader.BaseStream.Seek(0x8, SeekOrigin.Begin);
                UInt32 questNameLength = reader.ReadUInt32();
                Console.WriteLine("INFO: quest_name_len " + questNameLength);

                byte[] questNameBytes = Helper.readCharacterAmount(reader, questNameLength);
                writer.Write(questNameBytes);
                writer.Write(newLine);

                reader.BaseStream.Seek(0x2, SeekOrigin.Current);
                UInt32 questDescriptionLength = reader.ReadUInt32();
                Console.WriteLine("INFO: quest_desc_len " + questDescriptionLength);

                byte[] questDescriptionBytes = Helper.readCharacterAmount(reader, questDescriptionLength);
                //byte[] questDescriptionBytes = reader.ReadBytes((int)questDescriptionLength);
                writer.Write(questDescriptionBytes);
                writer.Write(newLine);

                reader.BaseStream.Seek(0x9, SeekOrigin.Current);
                UInt32 questFailureLength = reader.ReadUInt32();
                Console.WriteLine("INFO: quest_fail_len " + questFailureLength);

                byte[] questFailureBytes = Helper.readCharacterAmount(reader, questFailureLength);
                writer.Write(questFailureBytes);
                writer.Write(newLine);

                reader.BaseStream.Seek(0x6, SeekOrigin.Current);
                UInt32 questGiverLength = reader.ReadUInt32();
                Console.WriteLine("INFO: quest_giver_len " + questGiverLength);

                byte[] questGiverBytes = Helper.readCharacterAmount(reader, questGiverLength);
                writer.Write(questGiverBytes);
                writer.Write(newLine);

                UInt32 questLongDescLength = reader.ReadUInt32();
                Console.WriteLine("INFO: quest_longdesc_len " + questLongDescLength);

                byte[] questLongDescBytes = Helper.readCharacterAmount(reader, questLongDescLength);
                writer.Write(questLongDescBytes);
                writer.Write(newLine);
            }

            reader.Close();

            Console.WriteLine("INFO: Finished processing " + Path.GetFileName(input) + "!");
        }

        static void USQuestInput(string input)
        {
            string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".ustxt";
            BinaryReader reader = new BinaryReader(File.OpenRead(input));

            if (File.Exists(output))
                File.Delete(output);

            using (FileStream fsStream = new FileStream(output, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fsStream, Encoding.UTF8))
            {
                reader.BaseStream.Seek(0x8, SeekOrigin.Begin);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questNameLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_name_len " + questNameLength);
                    byte[] newLine = { 0x0D, 0x0A };
                    byte[] questNameBytes = Helper.readCharacterAmount(reader, questNameLength);
                    writer.Write(questNameBytes);
                    writer.Write(newLine);
                }

                reader.BaseStream.Seek(0x2, SeekOrigin.Current);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questDescLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_desc_len " + questDescLength);
                    byte[] newLine = { 0x0D, 0x0A };
                    byte[] questDescBytes = Helper.readCharacterAmount(reader, questDescLength);
                    writer.Write(questDescBytes);
                    writer.Write(newLine);
                }

                reader.BaseStream.Seek(0x18, SeekOrigin.Current);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questFailLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_fail_len " + questFailLength);
                    byte[] newLine = { 0x0D, 0x0A };
                    byte[] questFailBytes = Helper.readCharacterAmount(reader, questFailLength);
                    writer.Write(questFailBytes);
                    writer.Write(newLine);
                }

                reader.BaseStream.Seek(0x6, SeekOrigin.Current);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questGiverLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_giver_len " + questGiverLength);
                    byte[] newLine = { 0x0D, 0x0A };
                    byte[] questGiverBytes = Helper.readCharacterAmount(reader, questGiverLength);
                    writer.Write(questGiverBytes);
                    writer.Write(newLine);
                }

                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questLongDescLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_longdesc_len " + questLongDescLength);
                    byte[] newLine = { 0x0D, 0x0A };
                    byte[] questLongDescBytes = Helper.readCharacterAmount(reader, questLongDescLength);
                    writer.Write(questLongDescBytes);
                    writer.Write(newLine);
                }
            }

            reader.Close();

            Console.WriteLine("INFO: Finished processing " + Path.GetFileName(input) + "!");
        }

        static void createHeader(string input, int string_count, int table_size)
        {
            string headerOutput = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".header";
            BinaryReader headerReader;
            bool renameAndDelete = false;

            long input_size = new FileInfo(input).Length;
            UInt32 table_start;

            if (File.Exists(headerOutput))
            {
                offsetSetter(headerOutput);
                headerReader = new BinaryReader(File.OpenRead(headerOutput));
                input_size = new FileInfo(headerOutput).Length;
                headerOutput = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + "2.header";
                table_start = Convert.ToUInt32(input_size);
                renameAndDelete = true;
            }
            else
            {
                headerReader = new BinaryReader(File.OpenRead(input));
                // Get table_start which is where the header ends
                BinaryReader reader = new BinaryReader(File.OpenRead(input));
                reader.BaseStream.Seek(fileOffsets.t_size_offset, SeekOrigin.Begin);
                UInt32 read_table_size = reader.ReadUInt32();
                Console.WriteLine("INFO: table_size " + read_table_size);
                table_start = Convert.ToUInt32(input_size) - read_table_size;
            }

            using (FileStream fsStream = new FileStream(headerOutput, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fsStream, Encoding.UTF8))
            {
                byte[] byteHeader = new byte[(int)table_start];
                //headerReader.Read(byteHeader, 0, (int)table_start);
                headerReader.Read(byteHeader, 0, fileOffsets.s_count_offset); // 0x10 is hardcoded s_count_offset
                writer.Write(byteHeader, 0, fileOffsets.s_count_offset); // Read until s_count
                headerReader.Read(byteHeader, 0, 4);
                writer.Write(string_count); // Write my own s_count
                headerReader.Read(byteHeader, 0, 4);
                writer.Write(byteHeader, 0, 4); // Read 4 more
                headerReader.Read(byteHeader, 0, 4);
                writer.Write(table_size); // Write my own t_size

                int remainingHeaderSize = (int)table_start - (12 + fileOffsets.s_count_offset);
                headerReader.Read(byteHeader, 0, remainingHeaderSize);
                writer.Write(byteHeader, 0, remainingHeaderSize); // Read remaining header
                headerReader.Close();
            }

            if (renameAndDelete)
            {
                string orgHeader = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".header";
                File.Delete(orgHeader);
                File.Move(headerOutput, orgHeader);
            }
        }
    }
}
