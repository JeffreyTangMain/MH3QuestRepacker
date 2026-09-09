using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MH3QuestRepacker
{
    class Program
    {
        // Byte caps = the longest value each field takes across all 365 shipped
        // Japanese quests (romfs\quest\*.quest). Only LongDescCap is enforced:
        // in-game testing showed 203 bytes loads and 230 does not, regardless of
        // line count or width. The other four are advisory - working translations
        // exceed them with no ill effect.
        const int NameCap = 28;
        const int DescCap = 49;
        const int FailCap = 53;
        const int GiverCap = 22;
        const int LongDescCap = 216;

        const int LongDescIndex = 4;

        static readonly int[] FieldCaps = { NameCap, DescCap, FailCap, GiverCap, LongDescCap };

        static readonly string[] FieldNames =
        {
            "quest name", "objective", "failure conditions", "quest giver", "long description"
        };

        // Bytes of untouched data that follow each string in a JP quest file.
        static readonly int[] JpGaps = { 2, 9, 6, 0, 0 };

        // Same, for the 5-language US/EU layout (gaps fall after each group of 5).
        const int UsGapAfterDesc = 24;

        static readonly byte[] Magic = { 0x51, 0x54, 0x44, 0x53 }; // "QTDS"

        static int errorCount = 0;

        static int Main(string[] args)
        {
            string readinput = "Empty91564";

            foreach (var arg in args)
            {
                string input = arg;

                if (readinput.Equals("Empty91564"))
                {
                    Console.WriteLine("Type 1 for JP files, type 2 for anything else");
                    readinput = Console.ReadLine();
                }

                bool jp = readinput != null && readinput.Trim().Equals("1");

                try
                {
                    if (Path.GetExtension(input) == ".txt")
                    {
                        if (jp) TxtInput(input);
                        else USTxtInput(input);
                    }
                    else
                    {
                        if (jp) QuestInput(input);
                        else USQuestInput(input);
                    }
                }
                catch (Exception ex)
                {
                    Fail(Path.GetFileName(input) + ": " + ex.Message);
                }
            }

            if (errorCount > 0)
                Console.WriteLine("FINISHED WITH " + errorCount + " ERROR(S). No file was modified for those.");

            return errorCount == 0 ? 0 : 1;
        }

        // Path.GetDirectoryName returns "" for a bare filename, which would send
        // output to the drive root.
        static string Sibling(string input, string suffix)
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(input));
            return Path.Combine(dir, Path.GetFileNameWithoutExtension(input) + suffix);
        }

        static void Fail(string message)
        {
            errorCount++;
            Console.WriteLine("ERROR: " + message);
        }

        // --- shared validation ---

        // Throws instead of silently substituting '?' - that is how UTF-8 mojibake
        // got into q_11806.
        static Encoding StrictShiftJis()
        {
            return Encoding.GetEncoding("shift-jis",
                EncoderFallback.ExceptionFallback,
                DecoderFallback.ExceptionFallback);
        }

        static Encoding StrictUtf8()
        {
            return Encoding.GetEncoding("utf-8",
                EncoderFallback.ExceptionFallback,
                DecoderFallback.ExceptionFallback);
        }

        // Reads the whole .txt up front so nothing is written unless the input is
        // good. BOM detection is off: a BOM would silently switch encodings.
        static string[] ReadAllLinesChecked(string input, Encoding enc, int expected, string label)
        {
            byte[] raw = File.ReadAllBytes(input);
            if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF)
            {
                Fail(Path.GetFileName(input) + " starts with a UTF-8 BOM. Re-save it without a BOM "
                     + "(in the correct encoding) or every string will be mangled.");
                return null;
            }

            if (enc.WebName == "shift_jis" && LooksLikeUtf8(raw))
            {
                Fail(Path.GetFileName(input) + " looks like it was saved as UTF-8, not Shift-JIS. "
                     + "Re-save it as Shift-JIS (ANSI/Japanese) or accented characters will turn "
                     + "into garbage in game.");
                return null;
            }

            var lines = new List<string>();
            try
            {
                using (var reader = new StreamReader(input, enc, false))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        lines.Add(line);
                }
            }
            catch (DecoderFallbackException)
            {
                Fail(Path.GetFileName(input) + " is not valid " + enc.WebName
                     + ". Re-save it in that encoding. Nothing was written.");
                return null;
            }

            // A trailing newline produces one empty entry; that is normal.
            while (lines.Count > expected && lines[lines.Count - 1].Length == 0)
                lines.RemoveAt(lines.Count - 1);

            if (lines.Count < expected)
            {
                Fail(Path.GetFileName(input) + " has " + lines.Count + " line(s), expected "
                     + expected + " (" + label + "). Nothing was written.");
                return null;
            }

            return lines.ToArray();
        }

        // A JP .txt holds Shift-JIS, which is almost never also valid UTF-8. So a
        // file that decodes cleanly as UTF-8 *and* has non-ASCII bytes was saved
        // in the wrong encoding - the mistake that put mojibake in q_11806.
        static bool LooksLikeUtf8(byte[] raw)
        {
            bool nonAscii = false;
            foreach (byte b in raw)
                if (b >= 0x80) { nonAscii = true; break; }
            if (!nonAscii) return false;

            try
            {
                StrictUtf8().GetString(raw);
                return true;
            }
            catch (DecoderFallbackException)
            {
                return false;
            }
        }

        static bool CheckMagic(byte[] original, string originalFile)
        {
            if (original.Length < 8)
            {
                Fail(Path.GetFileName(originalFile) + " is only " + original.Length
                     + " bytes, too short to be a quest file.");
                return false;
            }

            for (int i = 0; i < Magic.Length; i++)
            {
                if (original[i] != Magic[i])
                {
                    Fail(Path.GetFileName(originalFile) + " does not start with the QTDS magic, "
                         + "so it is not a quest file.");
                    return false;
                }
            }

            return true;
        }

        // Enforces the long-description cap, warns on the rest. capIndex maps a
        // field to its cap so the 5-language US layout can share this.
        static bool CheckCaps(byte[][] fields, Func<int, int> capIndex, string input)
        {
            bool ok = true;

            for (int i = 0; i < fields.Length; i++)
            {
                int f = capIndex(i);
                int len = fields[i].Length;
                if (len <= FieldCaps[f]) continue;

                string where = Path.GetFileName(input) + ", " + FieldNames[f];

                if (f == LongDescIndex)
                {
                    Fail(where + " is " + len + " bytes, over the " + LongDescCap
                         + "-byte limit by " + (len - LongDescCap) + ". The quest will not load. "
                         + "Shorten the text - line breaks do not count against you, only bytes. "
                         + "Nothing was written.");
                    ok = false;
                }
                else
                {
                    Console.WriteLine("WARNING: " + where + " is " + len + " bytes, longer than any "
                                      + "original (" + FieldCaps[f] + "). This is allowed and should "
                                      + "still load, but may look wrong on screen.");
                }
            }

            return ok;
        }

        // Rebuilds a quest: header, then each string with a fresh length prefix,
        // with every gap field and the whole tail copied through untouched.
        static byte[] Rebuild(byte[] original, byte[][] fields, int[] gaps, string originalFile)
        {
            int pos = 8;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(original, 0, 8);

                for (int i = 0; i < fields.Length; i++)
                {
                    if (pos + 4 > original.Length)
                    {
                        Fail(Path.GetFileName(originalFile) + " ended early while reading the length of "
                             + "string " + (i + 1) + ".");
                        return null;
                    }

                    int oldLength = BitConverter.ToInt32(original, pos);
                    pos += 4;

                    if (oldLength < 0 || pos + oldLength > original.Length)
                    {
                        Fail(Path.GetFileName(originalFile) + " claims string " + (i + 1) + " is "
                             + oldLength + " bytes, which runs past the end of the file. "
                             + "It is not a quest file, or it is corrupt.");
                        return null;
                    }

                    pos += oldLength;

                    writer.Write(fields[i].Length);
                    writer.Write(fields[i]);

                    int gap = gaps[i];
                    if (gap > 0)
                    {
                        if (pos + gap > original.Length)
                        {
                            Fail(Path.GetFileName(originalFile) + " ended early in the gap after string "
                                 + (i + 1) + ".");
                            return null;
                        }

                        writer.Write(original, pos, gap);
                        pos += gap;
                    }
                }

                writer.Write(original, pos, original.Length - pos);
                writer.Flush();
                return ms.ToArray();
            }
        }

        // Encodes one line, turning <LINE> back into a newline. Null if a character
        // cannot be represented.
        static byte[] EncodeField(string line, string newline, Encoding enc, string input, string fieldName)
        {
            string text = line.Replace("<LINE>", newline);
            try
            {
                return enc.GetBytes(text);
            }
            catch (EncoderFallbackException ex)
            {
                Fail(Path.GetFileName(input) + ", " + fieldName + ": the character '" + ex.CharUnknown
                     + "' cannot be written in " + enc.WebName + ". Replace it with a plain ASCII "
                     + "equivalent (straight quotes, a hyphen instead of a dash). Nothing was written.");
                return null;
            }
        }

        static void Commit(string originalFile, byte[] result)
        {
            string temp = originalFile + ".tmp";
            File.WriteAllBytes(temp, result);
            if (File.Exists(originalFile))
                File.Delete(originalFile);
            File.Move(temp, originalFile);
        }

        // --- JP repack ---

        static void TxtInput(string input)
        {
            Encoding enc = StrictShiftJis();

            string[] lines = ReadAllLinesChecked(input, enc, 6,
                "an extension marker plus five strings");
            if (lines == null) return;

            string extension = lines[0];
            string originalFile = Sibling(input, extension);

            if (!File.Exists(originalFile))
            {
                Fail("cannot find the original quest file " + originalFile
                     + " (taken from line 1 of " + Path.GetFileName(input) + ").");
                return;
            }

            byte[] original = File.ReadAllBytes(originalFile);
            if (!CheckMagic(original, originalFile)) return;

            var fields = new byte[5][];
            for (int i = 0; i < 5; i++)
            {
                fields[i] = EncodeField(lines[i + 1], "\n", enc, input, FieldNames[i]);
                if (fields[i] == null) return;
            }

            if (!CheckCaps(fields, i => i, input)) return;

            byte[] result = Rebuild(original, fields, JpGaps, originalFile);
            if (result == null) return;

            Commit(originalFile, result);
            Console.WriteLine("INFO: wrote " + Path.GetFileName(originalFile) + " ("
                              + result.Length + " bytes, long description "
                              + fields[LongDescIndex].Length + "/" + LongDescCap + ").");
        }

        // --- US/EU repack (five languages per field) ---

        static void USTxtInput(string input)
        {
            Encoding enc = StrictUtf8();

            string[] lines = ReadAllLinesChecked(input, enc, 25,
                "five strings in five languages");
            if (lines == null) return;

            string originalFile = Sibling(input, ".quest");

            if (!File.Exists(originalFile))
            {
                Fail("cannot find the original quest file " + originalFile + ".");
                return;
            }

            byte[] original = File.ReadAllBytes(originalFile);
            if (!CheckMagic(original, originalFile)) return;

            var fields = new byte[25][];
            var gaps = new int[25];

            for (int i = 0; i < 25; i++)
            {
                int group = i / 5;
                fields[i] = EncodeField(lines[i], "\r\n", enc, input,
                    FieldNames[group] + " (language " + (i % 5 + 1) + ")");
                if (fields[i] == null) return;

                if (i % 5 == 4)
                {
                    if (group == 0) gaps[i] = JpGaps[0];
                    else if (group == 1) gaps[i] = UsGapAfterDesc;
                    else if (group == 2) gaps[i] = JpGaps[2];
                    else gaps[i] = 0;
                }
            }

            if (!CheckCaps(fields, i => i / 5, input)) return;

            byte[] result = Rebuild(original, fields, gaps, originalFile);
            if (result == null) return;

            Commit(originalFile, result);
            Console.WriteLine("INFO: wrote " + Path.GetFileName(originalFile) + " ("
                              + result.Length + " bytes).");
        }

        // --- dump ---

        static void QuestInput(string input)
        {
            string output = Sibling(input, ".txt");

            byte[] original = File.ReadAllBytes(input);
            if (!CheckMagic(original, input)) return;

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
                Console.WriteLine("INFO: quest_longdesc_len " + questLongDescLength
                                  + " (limit " + LongDescCap + ")");

                byte[] questLongDescBytes = Helper.readCharacterAmount(reader, questLongDescLength);
                writer.Write(questLongDescBytes);
                writer.Write(newLine);
            }

            reader.Close();

            Console.WriteLine("INFO: Finished processing " + Path.GetFileName(input) + "!");
        }

        static void USQuestInput(string input)
        {
            string output = Sibling(input, ".txt");
            BinaryReader reader = new BinaryReader(File.OpenRead(input));

            if (File.Exists(output))
                File.Delete(output);

            using (FileStream fsStream = new FileStream(output, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fsStream, Encoding.UTF8))
            {
                byte[] newLine = { 0x0D, 0x0A };

                reader.BaseStream.Seek(0x8, SeekOrigin.Begin);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questNameLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_name_len " + questNameLength);
                    byte[] questNameBytes = Helper.readCharacterAmount(reader, questNameLength);
                    writer.Write(questNameBytes);
                    writer.Write(newLine);
                }

                reader.BaseStream.Seek(0x2, SeekOrigin.Current);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questDescLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_desc_len " + questDescLength);
                    byte[] questDescBytes = Helper.readCharacterAmount(reader, questDescLength);
                    writer.Write(questDescBytes);
                    writer.Write(newLine);
                }

                reader.BaseStream.Seek(0x18, SeekOrigin.Current);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questFailLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_fail_len " + questFailLength);
                    byte[] questFailBytes = Helper.readCharacterAmount(reader, questFailLength);
                    writer.Write(questFailBytes);
                    writer.Write(newLine);
                }

                reader.BaseStream.Seek(0x6, SeekOrigin.Current);
                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questGiverLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_giver_len " + questGiverLength);
                    byte[] questGiverBytes = Helper.readCharacterAmount(reader, questGiverLength);
                    writer.Write(questGiverBytes);
                    writer.Write(newLine);
                }

                for (Int32 i = 0; i < 5; i++)
                {
                    UInt32 questLongDescLength = reader.ReadUInt32();
                    Console.WriteLine("INFO: quest_longdesc_len " + questLongDescLength);
                    byte[] questLongDescBytes = Helper.readCharacterAmount(reader, questLongDescLength);
                    writer.Write(questLongDescBytes);
                    writer.Write(newLine);
                }
            }

            reader.Close();

            Console.WriteLine("INFO: Finished processing " + Path.GetFileName(input) + "!");
        }

        public static void Pause()
        {
            Console.Write("Press any key to continue . . .");
            Console.ReadKey(true);
        }
    }
}
