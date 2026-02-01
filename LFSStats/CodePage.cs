/*
    LFSStat, Insim Replay statistics for Live For Speed Game
    Copyright (C) 2008 Jaroslav Èerný alias JackCY, Robert B. alias Gai-Luron and Monkster.
    Jack.SC7@gmail.com, lfsgailuron@free.fr

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either Version 3 of the License, or
    (at your option) any later Version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Text;

namespace InSim
{
    public class CodePage
    {
        public static int[] EToUni = {
            8364,0,8218,0,8222,8230,8224,8225,0,8240,352,8249,346,356,381,377,0,8216,8217,8220,8221,8226,8211,8212,0,8482,353,8250,347,357,382,378,160,711,728,321,164,260,166,167,168,169,350,171,172,173,174,379,176,177,731,322,180,181,182,183,184,261,351,187,317,733,318,380,340,193,194,258,196,313,262,199,268,201,280,203,282,205,206,270,272,323,327,211,212,336,214,215,344,366,218,368,220,221,354,223,341,225,226,259,228,314,263,231,269,233,281,235,283,237,238,271,273,324,328,243,244,337,246,247,345,367,250,369,252,253,355,729};
        public static int[] CToUni = {
            1026,1027,8218,1107,8222,8230,8224,8225,8364,8240,1033,8249,1034,1036,1035,1039,1106,8216,8217,8220,8221,8226,8211,
            8212,0,8482,1113,8250,1114,1116,1115,1119,
            160,1038,1118,1032,164,1168,166,167,1025,169,1028,171,172,173,174,1031,176,177,1030,1110,
            1169,181,182,183,1105,8470,1108,187,1112,1029,1109,1111,1040,1041,1042,1043,1044,1045,1046,1047,
            1048,1049,1050,1051,1052,1053,1054,1055,1056,1057,
            1058,1059,1060,1061,1062,1063,1064,1065,1066,1067,
            /*220*/1068,1069,1070,1071,1072,1073,1074,1075,1076,1077,
            /*230*/1078,1079,1080,1081,1082,1083,1084,1085,1086,1087,
            /*240*/1088,1089,1090,1091,1092,1093,1094,1095,1096,1097,
            /*250*/1098,1099,1100,1101,1102,1103};
        public static int[] LToUni = {
            8364,0,8218,402,8222,8230,8224,8225,710,8240,352,8249,
            /*140*/338,0,381,0,0,8216,8217,8220,8221,8226,
            /*150*/8211,8212,732,8482,353,8250,339,0,382,376,
            /*160*/160,161,162,163,164,165,166,167,168,169,
            /*170*/170,171,172,173,174,175,176,177,178,179,
            /*180*/180,181,182,183,184,185,186,187,188,189,
            /*190*/190,191,192,193,194,195,196,197,198,199,
            /*200*/200,201,202,203,204,205,206,207,208,209,
            /*210*/210,211,212,213,214,215,216,217,218,219,
            /*220*/220,221,222,223,224,225,226,227,228,229,
            /*230*/230,231,232,233,234,235,236,237,238,239,
            /*240*/240,241,242,243,244,245,246,247,248,249,
            /*250*/250,251,252,253,254,255
        };
        public static int[] GToUni = {
            /*128*/8364,0,8218,402,8222,8230,8224,8225,0,8240,0,8249,
            0,0,0,0,0,8216,8217,8220,8221,8226,
            /*150*/8211,8212,0,8482,0,8250,0,0,0,0,
            /*160*/160,901,902,163,164,165,166,167,168,169,
            0,171,172,173,174,8213,176,177,178,179,
            /*180*/900,181,182,183,904,905,906,187,908,189,
            /*190*/910,911,912,913,914,915,916,917,918,919,
            /*200*/920,921,922,923,924,925,926,927,928,929,
            /*210*/0,931,932,933,934,935,936,937,938,939,
            /*220*/940,941,942,943,944,945,946,947,948,949,
            /*230*/950,951,952,953,954,955,956,957,958,959,
            /*240*/960,961,962,963,964,965,966,967,968,969,
            /*250*/970,971,972,973,974,0
        };
        public static int[] TToUni = {
            /*128*/8364,0,8218,402,8222,8230,8224,8225,710,8240,352,8249,
            /*140*/338,0,0,0,0,8216,8217,8220,8221,8226,
            /*150*/8211,8212,732,8482,353,8250,339,0,0,376,
            /*160*/160,161,162,163,164,165,166,167,168,169,
            /*170*/170,171,172,173,174,175,176,177,178,179,
            /*180*/180,181,182,183,184,185,186,187,188,189,
            /*190*/190,191,192,193,194,195,196,197,198,199,
            /*200*/200,201,202,203,204,205,206,207,286,209,
            /*210*/210,211,212,213,214,215,216,217,218,219,
            /*220*/220,304,350,223,224,225,226,227,228,229,
            /*230*/230,231,232,233,234,235,236,237,238,239,
            /*240*/287,241,242,243,244,245,246,247,248,249,
            /*250*/250,251,252,305,351,255
        };
        public static int[] BToUni = {
            /*128*/8364,0,8218,0,8222,8230,8224,8225,0,8240,0,8249,
            0,168,711,184,0,8216,8217,8220,8221,8226,
            8211,8212,0,8482,0,8250,0,175,731,0,
            /*160*/160,0,162,163,164,0,166,167,216,169,
            /*170*/342,171,172,173,174,198,176,177,178,179,
            /*180*/180,181,182,183,248,185,343,187,188,189,
            /*190*/190,230,260,302,256,262,196,197,280,274,
            /*200*/268,201,377,278,290,310,298,315,352,323,
            /*210*/325,211,332,213,214,215,370,321,346,362,
            /*220*/220,379,381,223,261,303,257,263,228,229,
            /*230*/281,275,269,233,378,279,291,311,299,316,
            /*240*/353,324,326,243,333,245,246,247,371,322,
            /*250*/347,363,252,380,382,729
        };
        public static int[] JToUni = {
            /*128*/8364,0,0,0,0,8230,8224,8225,169,163,0,8249,
            0,0,0,0,0,0,0,0,0,8226,
            0,0,174,8482,127,8250,177,185,178,179,
            0,65377,65378,65379,65380,65381,65382,65383,65384,65385,
            /*170*/65386,65387,65388,65389,65390,65391,65392,65393,65394,65395,
            /*180*/65396,65397,65398,65399,65400,65401,65402,65403,65404,65405,
            /*190*/65406,65407,65408,65409,65410,65411,65412,65413,65414,65415,
            /*200*/65416,65417,65418,65419,65420,65421,65422,65423,65424,65425,
            /*210*/65426,65427,65428,65429,65430,65431,65432,65433,65434,65435,
            /*220*/65436,65437,65438,65439,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0
        };

        /// <summary>
        /// Gets unicode string from LFS encoded string (bytes).
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static string GetString(byte[] pack)
        {
            return GetString(pack, 0, pack.Length);
        }

        /// <summary>
        /// Gets unicode string from LFS encoded string (bytes).
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetString(byte[] pack,int offset,int len)
        {
            string temp = "";
            char codepage = 'L';
            bool specchar = false;

            for (int i = offset; i < offset + len; i++)
            {
                if (pack[i] == 0)
                    break;
                else if (pack[i] == '^')
                {
                    temp += (char)pack[i];
                    specchar = true;
                }
                else
                    if (pack[i] > 127)
                    {
                        //System.Console.WriteLine(pack[i]);

                        switch (codepage)
                        {
                            case 'E': //east european
                                temp += (char)EToUni[pack[i] - 128];
                                break;
                            case 'C': //cyrillic
                                temp += (char)CToUni[pack[i] - 128];
                                break;
                            case 'L':
                                temp += (char)LToUni[pack[i] - 128];
                                break;
                            case 'G':
                                temp += (char)GToUni[pack[i] - 128];
                                break;
                            case 'T':
                                temp += (char)TToUni[pack[i] - 128];
                                break;
                            case 'B':
                                temp += (char)BToUni[pack[i] - 128];
                                break;
                            case 'J':
                                temp += (char)JToUni[pack[i] - 128];
                                break;
                            default:
                                throw new System.Exception("Unknown codepage");
                        }
                    }
                    else
                    {
                        if(specchar)
                        {
                            switch (pack[i])
                            {
                                case (byte)'E':
                                case (byte)'C':
                                case (byte)'L':
                                case (byte)'G':
                                case (byte)'T':
                                case (byte)'B':
                                case (byte)'J':
                                    codepage = (char)pack[i];
                                    break;

                                default: //might be just color
                                    break;
                            }

                            specchar = false;
                        }

                        temp += (char)pack[i];
                    }
            }

            return temp;
        }

        public static void GetBytes(string s,int charIndex, int charCount,byte[] bs,int idx)
        {
            char codepage = 'L';
            bool specchar = false;

            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                if (s[i] == '^')
                {
                    bs[idx++] = (byte)s[i];
                    specchar = true;
                }
                else if (s[i] > 127)
                {
                    switch (codepage)
                    {
                        case 'E':
                            switch ((int)s[i])
                            {
                                case 8364: bs[idx++] = 128; break;
                                case 8218: bs[idx++] = 130; break;
                                case 8222: bs[idx++] = 132; break;
                                case 8230: bs[idx++] = 133; break;
                                case 8224: bs[idx++] = 134; break;
                                case 8225: bs[idx++] = 135; break;
                                case 8240: bs[idx++] = 137; break;
                                case 352: bs[idx++] = 138; break;
                                case 8249: bs[idx++] = 139; break;
                                case 346: bs[idx++] = 140; break;
                                case 356: bs[idx++] = 141; break;
                                case 381: bs[idx++] = 142; break;
                                case 377: bs[idx++] = 143; break;
                                case 8216: bs[idx++] = 145; break;
                                case 8217: bs[idx++] = 146; break;
                                case 8220: bs[idx++] = 147; break;
                                case 8221: bs[idx++] = 148; break;
                                case 8226: bs[idx++] = 149; break;
                                case 8211: bs[idx++] = 150; break;
                                case 8212: bs[idx++] = 151; break;
                                case 8482: bs[idx++] = 153; break;
                                case 353: bs[idx++] = 154; break;
                                case 8250: bs[idx++] = 155; break;
                                case 347: bs[idx++] = 156; break;
                                case 357: bs[idx++] = 157; break;
                                case 382: bs[idx++] = 158; break;
                                case 378: bs[idx++] = 159; break;
                                case 160: bs[idx++] = 160; break;
                                case 711: bs[idx++] = 161; break;
                                case 728: bs[idx++] = 162; break;
                                case 321: bs[idx++] = 163; break;
                                case 164: bs[idx++] = 164; break;
                                case 260: bs[idx++] = 165; break;
                                case 166: bs[idx++] = 166; break;
                                case 167: bs[idx++] = 167; break;
                                case 168: bs[idx++] = 168; break;
                                case 169: bs[idx++] = 169; break;
                                case 350: bs[idx++] = 170; break;
                                case 171: bs[idx++] = 171; break;
                                case 172: bs[idx++] = 172; break;
                                case 173: bs[idx++] = 173; break;
                                case 174: bs[idx++] = 174; break;
                                case 379: bs[idx++] = 175; break;
                                case 176: bs[idx++] = 176; break;
                                case 177: bs[idx++] = 177; break;
                                case 731: bs[idx++] = 178; break;
                                case 322: bs[idx++] = 179; break;
                                case 180: bs[idx++] = 180; break;
                                case 181: bs[idx++] = 181; break;
                                case 182: bs[idx++] = 182; break;
                                case 183: bs[idx++] = 183; break;
                                case 184: bs[idx++] = 184; break;
                                case 261: bs[idx++] = 185; break;
                                case 351: bs[idx++] = 186; break;
                                case 187: bs[idx++] = 187; break;
                                case 317: bs[idx++] = 188; break;
                                case 733: bs[idx++] = 189; break;
                                case 318: bs[idx++] = 190; break;
                                case 380: bs[idx++] = 191; break;
                                case 340: bs[idx++] = 192; break;
                                case 193: bs[idx++] = 193; break;
                                case 194: bs[idx++] = 194; break;
                                case 258: bs[idx++] = 195; break;
                                case 196: bs[idx++] = 196; break;
                                case 313: bs[idx++] = 197; break;
                                case 262: bs[idx++] = 198; break;
                                case 199: bs[idx++] = 199; break;
                                case 268: bs[idx++] = 200; break;
                                case 201: bs[idx++] = 201; break;
                                case 280: bs[idx++] = 202; break;
                                case 203: bs[idx++] = 203; break;
                                case 282: bs[idx++] = 204; break;
                                case 205: bs[idx++] = 205; break;
                                case 206: bs[idx++] = 206; break;
                                case 270: bs[idx++] = 207; break;
                                case 272: bs[idx++] = 208; break;
                                case 323: bs[idx++] = 209; break;
                                case 327: bs[idx++] = 210; break;
                                case 211: bs[idx++] = 211; break;
                                case 212: bs[idx++] = 212; break;
                                case 336: bs[idx++] = 213; break;
                                case 214: bs[idx++] = 214; break;
                                case 215: bs[idx++] = 215; break;
                                case 344: bs[idx++] = 216; break;
                                case 366: bs[idx++] = 217; break;
                                case 218: bs[idx++] = 218; break;
                                case 368: bs[idx++] = 219; break;
                                case 220: bs[idx++] = 220; break;
                                case 221: bs[idx++] = 221; break;
                                case 354: bs[idx++] = 222; break;
                                case 223: bs[idx++] = 223; break;
                                case 341: bs[idx++] = 224; break;
                                case 225: bs[idx++] = 225; break;
                                case 226: bs[idx++] = 226; break;
                                case 259: bs[idx++] = 227; break;
                                case 228: bs[idx++] = 228; break;
                                case 314: bs[idx++] = 229; break;
                                case 263: bs[idx++] = 230; break;
                                case 231: bs[idx++] = 231; break;
                                case 269: bs[idx++] = 232; break;
                                case 233: bs[idx++] = 233; break;
                                case 281: bs[idx++] = 234; break;
                                case 235: bs[idx++] = 235; break;
                                case 283: bs[idx++] = 236; break;
                                case 237: bs[idx++] = 237; break;
                                case 238: bs[idx++] = 238; break;
                                case 271: bs[idx++] = 239; break;
                                case 273: bs[idx++] = 240; break;
                                case 324: bs[idx++] = 241; break;
                                case 328: bs[idx++] = 242; break;
                                case 243: bs[idx++] = 243; break;
                                case 244: bs[idx++] = 244; break;
                                case 337: bs[idx++] = 245; break;
                                case 246: bs[idx++] = 246; break;
                                case 247: bs[idx++] = 247; break;
                                case 345: bs[idx++] = 248; break;
                                case 367: bs[idx++] = 249; break;
                                case 250: bs[idx++] = 250; break;
                                case 369: bs[idx++] = 251; break;
                                case 252: bs[idx++] = 252; break;
                                case 253: bs[idx++] = 253; break;
                                case 355: bs[idx++] = 254; break;
                                case 729: bs[idx++] = 255; break;
                                
                                default:
                                    bs[idx++] = (byte)'?';
                                    break;
                                    //throw new System.Exception("Could not convert from unicode to CodePage E");
                            }
                            break;

                        case 'C':
                            switch ((int)s[i])
                            {
                                case 1026: bs[idx++] = 128; break;
                                case 1027: bs[idx++] = 129; break;
                                case 8218: bs[idx++] = 130; break;
                                case 1107: bs[idx++] = 131; break;
                                case 8222: bs[idx++] = 132; break;
                                case 8230: bs[idx++] = 133; break;
                                case 8224: bs[idx++] = 134; break;
                                case 8225: bs[idx++] = 135; break;
                                case 8364: bs[idx++] = 136; break;
                                case 8240: bs[idx++] = 137; break;
                                case 1033: bs[idx++] = 138; break;
                                case 8249: bs[idx++] = 139; break;
                                case 1034: bs[idx++] = 140; break;
                                case 1036: bs[idx++] = 141; break;
                                case 1035: bs[idx++] = 142; break;
                                case 1039: bs[idx++] = 143; break;
                                case 1106: bs[idx++] = 144; break;
                                case 8216: bs[idx++] = 145; break;
                                case 8217: bs[idx++] = 146; break;
                                case 8220: bs[idx++] = 147; break;
                                case 8221: bs[idx++] = 148; break;
                                case 8226: bs[idx++] = 149; break;
                                case 8211: bs[idx++] = 150; break;
                                case 8212: bs[idx++] = 151; break;
                                case 8482: bs[idx++] = 153; break;
                                case 1113: bs[idx++] = 154; break;
                                case 8250: bs[idx++] = 155; break;
                                case 1114: bs[idx++] = 156; break;
                                case 1116: bs[idx++] = 157; break;
                                case 1115: bs[idx++] = 158; break;
                                case 1119: bs[idx++] = 159; break;
                                case 160: bs[idx++] = 160; break;
                                case 1038: bs[idx++] = 161; break;
                                case 1118: bs[idx++] = 162; break;
                                case 1032: bs[idx++] = 163; break;
                                case 164: bs[idx++] = 164; break;
                                case 1168: bs[idx++] = 165; break;
                                case 166: bs[idx++] = 166; break;
                                case 167: bs[idx++] = 167; break;
                                case 1025: bs[idx++] = 168; break;
                                case 169: bs[idx++] = 169; break;
                                case 1028: bs[idx++] = 170; break;
                                case 171: bs[idx++] = 171; break;
                                case 172: bs[idx++] = 172; break;
                                case 173: bs[idx++] = 173; break;
                                case 174: bs[idx++] = 174; break;
                                case 1031: bs[idx++] = 175; break;
                                case 176: bs[idx++] = 176; break;
                                case 177: bs[idx++] = 177; break;
                                case 1030: bs[idx++] = 178; break;
                                case 1110: bs[idx++] = 179; break;
                                case 1169: bs[idx++] = 180; break;
                                case 181: bs[idx++] = 181; break;
                                case 182: bs[idx++] = 182; break;
                                case 183: bs[idx++] = 183; break;
                                case 1105: bs[idx++] = 184; break;
                                case 8470: bs[idx++] = 185; break;
                                case 1108: bs[idx++] = 186; break;
                                case 187: bs[idx++] = 187; break;
                                case 1112: bs[idx++] = 188; break;
                                case 1029: bs[idx++] = 189; break;
                                case 1109: bs[idx++] = 190; break;
                                case 1111: bs[idx++] = 191; break;
                                case 1040: bs[idx++] = 192; break;
                                case 1041: bs[idx++] = 193; break;
                                case 1042: bs[idx++] = 194; break;
                                case 1043: bs[idx++] = 195; break;
                                case 1044: bs[idx++] = 196; break;
                                case 1045: bs[idx++] = 197; break;
                                case 1046: bs[idx++] = 198; break;
                                case 1047: bs[idx++] = 199; break;
                                case 1048: bs[idx++] = 200; break;
                                case 1049: bs[idx++] = 201; break;
                                case 1050: bs[idx++] = 202; break;
                                case 1051: bs[idx++] = 203; break;
                                case 1052: bs[idx++] = 204; break;
                                case 1053: bs[idx++] = 205; break;
                                case 1054: bs[idx++] = 206; break;
                                case 1055: bs[idx++] = 207; break;
                                case 1056: bs[idx++] = 208; break;
                                case 1057: bs[idx++] = 209; break;
                                case 1058: bs[idx++] = 210; break;
                                case 1059: bs[idx++] = 211; break;
                                case 1060: bs[idx++] = 212; break;
                                case 1061: bs[idx++] = 213; break;
                                case 1062: bs[idx++] = 214; break;
                                case 1063: bs[idx++] = 215; break;
                                case 1064: bs[idx++] = 216; break;
                                case 1065: bs[idx++] = 217; break;
                                case 1066: bs[idx++] = 218; break;
                                case 1067: bs[idx++] = 219; break;
                                case 1068: bs[idx++] = 220; break;
                                case 1069: bs[idx++] = 221; break;
                                case 1070: bs[idx++] = 222; break;
                                case 1071: bs[idx++] = 223; break;
                                case 1072: bs[idx++] = 224; break;
                                case 1073: bs[idx++] = 225; break;
                                case 1074: bs[idx++] = 226; break;
                                case 1075: bs[idx++] = 227; break;
                                case 1076: bs[idx++] = 228; break;
                                case 1077: bs[idx++] = 229; break;
                                case 1078: bs[idx++] = 230; break;
                                case 1079: bs[idx++] = 231; break;
                                case 1080: bs[idx++] = 232; break;
                                case 1081: bs[idx++] = 233; break;
                                case 1082: bs[idx++] = 234; break;
                                case 1083: bs[idx++] = 235; break;
                                case 1084: bs[idx++] = 236; break;
                                case 1085: bs[idx++] = 237; break;
                                case 1086: bs[idx++] = 238; break;
                                case 1087: bs[idx++] = 239; break;
                                case 1088: bs[idx++] = 240; break;
                                case 1089: bs[idx++] = 241; break;
                                case 1090: bs[idx++] = 242; break;
                                case 1091: bs[idx++] = 243; break;
                                case 1092: bs[idx++] = 244; break;
                                case 1093: bs[idx++] = 245; break;
                                case 1094: bs[idx++] = 246; break;
                                case 1095: bs[idx++] = 247; break;
                                case 1096: bs[idx++] = 248; break;
                                case 1097: bs[idx++] = 249; break;
                                case 1098: bs[idx++] = 250; break;
                                case 1099: bs[idx++] = 251; break;
                                case 1100: bs[idx++] = 252; break;
                                case 1101: bs[idx++] = 253; break;
                                case 1102: bs[idx++] = 254; break;
                                case 1103: bs[idx++] = 255; break;

                                default:
                                    bs[idx++] = (byte)'?';
                                    break;
                                    //throw new System.Exception("Could not convert from unicode to CodePage E");
                            }
                            break;

                        case 'L':
                            switch ((int)s[i])
                            {
                                case 8364: bs[idx++] = 128; break;
                                case 8218: bs[idx++] = 130; break;
                                case 402: bs[idx++] = 131; break;
                                case 8222: bs[idx++] = 132; break;
                                case 8230: bs[idx++] = 133; break;
                                case 8224: bs[idx++] = 134; break;
                                case 8225: bs[idx++] = 135; break;
                                case 710: bs[idx++] = 136; break;
                                case 8240: bs[idx++] = 137; break;
                                case 352: bs[idx++] = 138; break;
                                case 8249: bs[idx++] = 139; break;
                                case 338: bs[idx++] = 140; break;
                                case 381: bs[idx++] = 142; break;
                                case 8216: bs[idx++] = 145; break;
                                case 8217: bs[idx++] = 146; break;
                                case 8220: bs[idx++] = 147; break;
                                case 8221: bs[idx++] = 148; break;
                                case 8226: bs[idx++] = 149; break;
                                case 8211: bs[idx++] = 150; break;
                                case 8212: bs[idx++] = 151; break;
                                case 732: bs[idx++] = 152; break;
                                case 8482: bs[idx++] = 153; break;
                                case 353: bs[idx++] = 154; break;
                                case 8250: bs[idx++] = 155; break;
                                case 339: bs[idx++] = 156; break;
                                case 382: bs[idx++] = 158; break;
                                case 376: bs[idx++] = 159; break;
                                case 160: bs[idx++] = 160; break;
                                case 161: bs[idx++] = 161; break;
                                case 162: bs[idx++] = 162; break;
                                case 163: bs[idx++] = 163; break;
                                case 164: bs[idx++] = 164; break;
                                case 165: bs[idx++] = 165; break;
                                case 166: bs[idx++] = 166; break;
                                case 167: bs[idx++] = 167; break;
                                case 168: bs[idx++] = 168; break;
                                case 169: bs[idx++] = 169; break;
                                case 170: bs[idx++] = 170; break;
                                case 171: bs[idx++] = 171; break;
                                case 172: bs[idx++] = 172; break;
                                case 173: bs[idx++] = 173; break;
                                case 174: bs[idx++] = 174; break;
                                case 175: bs[idx++] = 175; break;
                                case 176: bs[idx++] = 176; break;
                                case 177: bs[idx++] = 177; break;
                                case 178: bs[idx++] = 178; break;
                                case 179: bs[idx++] = 179; break;
                                case 180: bs[idx++] = 180; break;
                                case 181: bs[idx++] = 181; break;
                                case 182: bs[idx++] = 182; break;
                                case 183: bs[idx++] = 183; break;
                                case 184: bs[idx++] = 184; break;
                                case 185: bs[idx++] = 185; break;
                                case 186: bs[idx++] = 186; break;
                                case 187: bs[idx++] = 187; break;
                                case 188: bs[idx++] = 188; break;
                                case 189: bs[idx++] = 189; break;
                                case 190: bs[idx++] = 190; break;
                                case 191: bs[idx++] = 191; break;
                                case 192: bs[idx++] = 192; break;
                                case 193: bs[idx++] = 193; break;
                                case 194: bs[idx++] = 194; break;
                                case 195: bs[idx++] = 195; break;
                                case 196: bs[idx++] = 196; break;
                                case 197: bs[idx++] = 197; break;
                                case 198: bs[idx++] = 198; break;
                                case 199: bs[idx++] = 199; break;
                                case 200: bs[idx++] = 200; break;
                                case 201: bs[idx++] = 201; break;
                                case 202: bs[idx++] = 202; break;
                                case 203: bs[idx++] = 203; break;
                                case 204: bs[idx++] = 204; break;
                                case 205: bs[idx++] = 205; break;
                                case 206: bs[idx++] = 206; break;
                                case 207: bs[idx++] = 207; break;
                                case 208: bs[idx++] = 208; break;
                                case 209: bs[idx++] = 209; break;
                                case 210: bs[idx++] = 210; break;
                                case 211: bs[idx++] = 211; break;
                                case 212: bs[idx++] = 212; break;
                                case 213: bs[idx++] = 213; break;
                                case 214: bs[idx++] = 214; break;
                                case 215: bs[idx++] = 215; break;
                                case 216: bs[idx++] = 216; break;
                                case 217: bs[idx++] = 217; break;
                                case 218: bs[idx++] = 218; break;
                                case 219: bs[idx++] = 219; break;
                                case 220: bs[idx++] = 220; break;
                                case 221: bs[idx++] = 221; break;
                                case 222: bs[idx++] = 222; break;
                                case 223: bs[idx++] = 223; break;
                                case 224: bs[idx++] = 224; break;
                                case 225: bs[idx++] = 225; break;
                                case 226: bs[idx++] = 226; break;
                                case 227: bs[idx++] = 227; break;
                                case 228: bs[idx++] = 228; break;
                                case 229: bs[idx++] = 229; break;
                                case 230: bs[idx++] = 230; break;
                                case 231: bs[idx++] = 231; break;
                                case 232: bs[idx++] = 232; break;
                                case 233: bs[idx++] = 233; break;
                                case 234: bs[idx++] = 234; break;
                                case 235: bs[idx++] = 235; break;
                                case 236: bs[idx++] = 236; break;
                                case 237: bs[idx++] = 237; break;
                                case 238: bs[idx++] = 238; break;
                                case 239: bs[idx++] = 239; break;
                                case 240: bs[idx++] = 240; break;
                                case 241: bs[idx++] = 241; break;
                                case 242: bs[idx++] = 242; break;
                                case 243: bs[idx++] = 243; break;
                                case 244: bs[idx++] = 244; break;
                                case 245: bs[idx++] = 245; break;
                                case 246: bs[idx++] = 246; break;
                                case 247: bs[idx++] = 247; break;
                                case 248: bs[idx++] = 248; break;
                                case 249: bs[idx++] = 249; break;
                                case 250: bs[idx++] = 250; break;
                                case 251: bs[idx++] = 251; break;
                                case 252: bs[idx++] = 252; break;
                                case 253: bs[idx++] = 253; break;
                                case 254: bs[idx++] = 254; break;
                                case 255: bs[idx++] = 255; break;

                                default:
                                    bs[idx++] = (byte)'?';
                                    break;
                                //throw new System.Exception("Could not convert from unicode to CodePage E");
                            }
                            break;
                        case 'G':
                            switch ((int)s[i])
                            {
                                case 8364: bs[idx++] = 128; break;
                                case 8218: bs[idx++] = 130; break;
                                case 402: bs[idx++] = 131; break;
                                case 8222: bs[idx++] = 132; break;
                                case 8230: bs[idx++] = 133; break;
                                case 8224: bs[idx++] = 134; break;
                                case 8225: bs[idx++] = 135; break;
                                case 8240: bs[idx++] = 137; break;
                                case 8249: bs[idx++] = 139; break;
                                case 8216: bs[idx++] = 145; break;
                                case 8217: bs[idx++] = 146; break;
                                case 8220: bs[idx++] = 147; break;
                                case 8221: bs[idx++] = 148; break;
                                case 8226: bs[idx++] = 149; break;
                                case 8211: bs[idx++] = 150; break;
                                case 8212: bs[idx++] = 151; break;
                                case 8482: bs[idx++] = 153; break;
                                case 8250: bs[idx++] = 155; break;
                                case 160: bs[idx++] = 160; break;
                                case 901: bs[idx++] = 161; break;
                                case 902: bs[idx++] = 162; break;
                                case 163: bs[idx++] = 163; break;
                                case 164: bs[idx++] = 164; break;
                                case 165: bs[idx++] = 165; break;
                                case 166: bs[idx++] = 166; break;
                                case 167: bs[idx++] = 167; break;
                                case 168: bs[idx++] = 168; break;
                                case 169: bs[idx++] = 169; break;
                                case 171: bs[idx++] = 171; break;
                                case 172: bs[idx++] = 172; break;
                                case 173: bs[idx++] = 173; break;
                                case 174: bs[idx++] = 174; break;
                                case 8213: bs[idx++] = 175; break;
                                case 176: bs[idx++] = 176; break;
                                case 177: bs[idx++] = 177; break;
                                case 178: bs[idx++] = 178; break;
                                case 179: bs[idx++] = 179; break;
                                case 900: bs[idx++] = 180; break;
                                case 181: bs[idx++] = 181; break;
                                case 182: bs[idx++] = 182; break;
                                case 183: bs[idx++] = 183; break;
                                case 904: bs[idx++] = 184; break;
                                case 905: bs[idx++] = 185; break;
                                case 906: bs[idx++] = 186; break;
                                case 187: bs[idx++] = 187; break;
                                case 908: bs[idx++] = 188; break;
                                case 189: bs[idx++] = 189; break;
                                case 910: bs[idx++] = 190; break;
                                case 911: bs[idx++] = 191; break;
                                case 912: bs[idx++] = 192; break;
                                case 913: bs[idx++] = 193; break;
                                case 914: bs[idx++] = 194; break;
                                case 915: bs[idx++] = 195; break;
                                case 916: bs[idx++] = 196; break;
                                case 917: bs[idx++] = 197; break;
                                case 918: bs[idx++] = 198; break;
                                case 919: bs[idx++] = 199; break;
                                case 920: bs[idx++] = 200; break;
                                case 921: bs[idx++] = 201; break;
                                case 922: bs[idx++] = 202; break;
                                case 923: bs[idx++] = 203; break;
                                case 924: bs[idx++] = 204; break;
                                case 925: bs[idx++] = 205; break;
                                case 926: bs[idx++] = 206; break;
                                case 927: bs[idx++] = 207; break;
                                case 928: bs[idx++] = 208; break;
                                case 929: bs[idx++] = 209; break;
                                case 931: bs[idx++] = 211; break;
                                case 932: bs[idx++] = 212; break;
                                case 933: bs[idx++] = 213; break;
                                case 934: bs[idx++] = 214; break;
                                case 935: bs[idx++] = 215; break;
                                case 936: bs[idx++] = 216; break;
                                case 937: bs[idx++] = 217; break;
                                case 938: bs[idx++] = 218; break;
                                case 939: bs[idx++] = 219; break;
                                case 940: bs[idx++] = 220; break;
                                case 941: bs[idx++] = 221; break;
                                case 942: bs[idx++] = 222; break;
                                case 943: bs[idx++] = 223; break;
                                case 944: bs[idx++] = 224; break;
                                case 945: bs[idx++] = 225; break;
                                case 946: bs[idx++] = 226; break;
                                case 947: bs[idx++] = 227; break;
                                case 948: bs[idx++] = 228; break;
                                case 949: bs[idx++] = 229; break;
                                case 950: bs[idx++] = 230; break;
                                case 951: bs[idx++] = 231; break;
                                case 952: bs[idx++] = 232; break;
                                case 953: bs[idx++] = 233; break;
                                case 954: bs[idx++] = 234; break;
                                case 955: bs[idx++] = 235; break;
                                case 956: bs[idx++] = 236; break;
                                case 957: bs[idx++] = 237; break;
                                case 958: bs[idx++] = 238; break;
                                case 959: bs[idx++] = 239; break;
                                case 960: bs[idx++] = 240; break;
                                case 961: bs[idx++] = 241; break;
                                case 962: bs[idx++] = 242; break;
                                case 963: bs[idx++] = 243; break;
                                case 964: bs[idx++] = 244; break;
                                case 965: bs[idx++] = 245; break;
                                case 966: bs[idx++] = 246; break;
                                case 967: bs[idx++] = 247; break;
                                case 968: bs[idx++] = 248; break;
                                case 969: bs[idx++] = 249; break;
                                case 970: bs[idx++] = 250; break;
                                case 971: bs[idx++] = 251; break;
                                case 972: bs[idx++] = 252; break;
                                case 973: bs[idx++] = 253; break;
                                case 974: bs[idx++] = 254; break;

                                default:
                                    bs[idx++] = (byte)'?';
                                    break;
                                //throw new System.Exception("Could not convert from unicode to CodePage E");
                            }
                            break;
                        case 'T':
                            switch ((int)s[i])
                            {
                                case 8364: bs[idx++] = 128; break;
                                case 8218: bs[idx++] = 130; break;
                                case 402: bs[idx++] = 131; break;
                                case 8222: bs[idx++] = 132; break;
                                case 8230: bs[idx++] = 133; break;
                                case 8224: bs[idx++] = 134; break;
                                case 8225: bs[idx++] = 135; break;
                                case 710: bs[idx++] = 136; break;
                                case 8240: bs[idx++] = 137; break;
                                case 352: bs[idx++] = 138; break;
                                case 8249: bs[idx++] = 139; break;
                                case 338: bs[idx++] = 140; break;
                                case 8216: bs[idx++] = 145; break;
                                case 8217: bs[idx++] = 146; break;
                                case 8220: bs[idx++] = 147; break;
                                case 8221: bs[idx++] = 148; break;
                                case 8226: bs[idx++] = 149; break;
                                case 8211: bs[idx++] = 150; break;
                                case 8212: bs[idx++] = 151; break;
                                case 732: bs[idx++] = 152; break;
                                case 8482: bs[idx++] = 153; break;
                                case 353: bs[idx++] = 154; break;
                                case 8250: bs[idx++] = 155; break;
                                case 339: bs[idx++] = 156; break;
                                case 376: bs[idx++] = 159; break;
                                case 160: bs[idx++] = 160; break;
                                case 161: bs[idx++] = 161; break;
                                case 162: bs[idx++] = 162; break;
                                case 163: bs[idx++] = 163; break;
                                case 164: bs[idx++] = 164; break;
                                case 165: bs[idx++] = 165; break;
                                case 166: bs[idx++] = 166; break;
                                case 167: bs[idx++] = 167; break;
                                case 168: bs[idx++] = 168; break;
                                case 169: bs[idx++] = 169; break;
                                case 170: bs[idx++] = 170; break;
                                case 171: bs[idx++] = 171; break;
                                case 172: bs[idx++] = 172; break;
                                case 173: bs[idx++] = 173; break;
                                case 174: bs[idx++] = 174; break;
                                case 175: bs[idx++] = 175; break;
                                case 176: bs[idx++] = 176; break;
                                case 177: bs[idx++] = 177; break;
                                case 178: bs[idx++] = 178; break;
                                case 179: bs[idx++] = 179; break;
                                case 180: bs[idx++] = 180; break;
                                case 181: bs[idx++] = 181; break;
                                case 182: bs[idx++] = 182; break;
                                case 183: bs[idx++] = 183; break;
                                case 184: bs[idx++] = 184; break;
                                case 185: bs[idx++] = 185; break;
                                case 186: bs[idx++] = 186; break;
                                case 187: bs[idx++] = 187; break;
                                case 188: bs[idx++] = 188; break;
                                case 189: bs[idx++] = 189; break;
                                case 190: bs[idx++] = 190; break;
                                case 191: bs[idx++] = 191; break;
                                case 192: bs[idx++] = 192; break;
                                case 193: bs[idx++] = 193; break;
                                case 194: bs[idx++] = 194; break;
                                case 195: bs[idx++] = 195; break;
                                case 196: bs[idx++] = 196; break;
                                case 197: bs[idx++] = 197; break;
                                case 198: bs[idx++] = 198; break;
                                case 199: bs[idx++] = 199; break;
                                case 200: bs[idx++] = 200; break;
                                case 201: bs[idx++] = 201; break;
                                case 202: bs[idx++] = 202; break;
                                case 203: bs[idx++] = 203; break;
                                case 204: bs[idx++] = 204; break;
                                case 205: bs[idx++] = 205; break;
                                case 206: bs[idx++] = 206; break;
                                case 207: bs[idx++] = 207; break;
                                case 286: bs[idx++] = 208; break;
                                case 209: bs[idx++] = 209; break;
                                case 210: bs[idx++] = 210; break;
                                case 211: bs[idx++] = 211; break;
                                case 212: bs[idx++] = 212; break;
                                case 213: bs[idx++] = 213; break;
                                case 214: bs[idx++] = 214; break;
                                case 215: bs[idx++] = 215; break;
                                case 216: bs[idx++] = 216; break;
                                case 217: bs[idx++] = 217; break;
                                case 218: bs[idx++] = 218; break;
                                case 219: bs[idx++] = 219; break;
                                case 220: bs[idx++] = 220; break;
                                case 304: bs[idx++] = 221; break;
                                case 350: bs[idx++] = 222; break;
                                case 223: bs[idx++] = 223; break;
                                case 224: bs[idx++] = 224; break;
                                case 225: bs[idx++] = 225; break;
                                case 226: bs[idx++] = 226; break;
                                case 227: bs[idx++] = 227; break;
                                case 228: bs[idx++] = 228; break;
                                case 229: bs[idx++] = 229; break;
                                case 230: bs[idx++] = 230; break;
                                case 231: bs[idx++] = 231; break;
                                case 232: bs[idx++] = 232; break;
                                case 233: bs[idx++] = 233; break;
                                case 234: bs[idx++] = 234; break;
                                case 235: bs[idx++] = 235; break;
                                case 236: bs[idx++] = 236; break;
                                case 237: bs[idx++] = 237; break;
                                case 238: bs[idx++] = 238; break;
                                case 239: bs[idx++] = 239; break;
                                case 287: bs[idx++] = 240; break;
                                case 241: bs[idx++] = 241; break;
                                case 242: bs[idx++] = 242; break;
                                case 243: bs[idx++] = 243; break;
                                case 244: bs[idx++] = 244; break;
                                case 245: bs[idx++] = 245; break;
                                case 246: bs[idx++] = 246; break;
                                case 247: bs[idx++] = 247; break;
                                case 248: bs[idx++] = 248; break;
                                case 249: bs[idx++] = 249; break;
                                case 250: bs[idx++] = 250; break;
                                case 251: bs[idx++] = 251; break;
                                case 252: bs[idx++] = 252; break;
                                case 305: bs[idx++] = 253; break;
                                case 351: bs[idx++] = 254; break;
                                case 255: bs[idx++] = 255; break;

                                default:
                                    bs[idx++] = (byte)'?';
                                    break;
                                //throw new System.Exception("Could not convert from unicode to CodePage E");
                            }
                            break;
                        case 'B':
                            switch ((int)s[i])
                            {
                                case 8364: bs[idx++] = 128; break;
                                case 8218: bs[idx++] = 130; break;
                                case 8222: bs[idx++] = 132; break;
                                case 8230: bs[idx++] = 133; break;
                                case 8224: bs[idx++] = 134; break;
                                case 8225: bs[idx++] = 135; break;
                                case 8240: bs[idx++] = 137; break;
                                case 8249: bs[idx++] = 139; break;
                                case 168: bs[idx++] = 141; break;
                                case 711: bs[idx++] = 142; break;
                                case 184: bs[idx++] = 143; break;
                                case 8216: bs[idx++] = 145; break;
                                case 8217: bs[idx++] = 146; break;
                                case 8220: bs[idx++] = 147; break;
                                case 8221: bs[idx++] = 148; break;
                                case 8226: bs[idx++] = 149; break;
                                case 8211: bs[idx++] = 150; break;
                                case 8212: bs[idx++] = 151; break;
                                case 8482: bs[idx++] = 153; break;
                                case 8250: bs[idx++] = 155; break;
                                case 175: bs[idx++] = 157; break;
                                case 731: bs[idx++] = 158; break;
                                case 160: bs[idx++] = 160; break;
                                case 162: bs[idx++] = 162; break;
                                case 163: bs[idx++] = 163; break;
                                case 164: bs[idx++] = 164; break;
                                case 166: bs[idx++] = 166; break;
                                case 167: bs[idx++] = 167; break;
                                case 216: bs[idx++] = 168; break;
                                case 169: bs[idx++] = 169; break;
                                case 342: bs[idx++] = 170; break;
                                case 171: bs[idx++] = 171; break;
                                case 172: bs[idx++] = 172; break;
                                case 173: bs[idx++] = 173; break;
                                case 174: bs[idx++] = 174; break;
                                case 198: bs[idx++] = 175; break;
                                case 176: bs[idx++] = 176; break;
                                case 177: bs[idx++] = 177; break;
                                case 178: bs[idx++] = 178; break;
                                case 179: bs[idx++] = 179; break;
                                case 180: bs[idx++] = 180; break;
                                case 181: bs[idx++] = 181; break;
                                case 182: bs[idx++] = 182; break;
                                case 183: bs[idx++] = 183; break;
                                case 248: bs[idx++] = 184; break;
                                case 185: bs[idx++] = 185; break;
                                case 343: bs[idx++] = 186; break;
                                case 187: bs[idx++] = 187; break;
                                case 188: bs[idx++] = 188; break;
                                case 189: bs[idx++] = 189; break;
                                case 190: bs[idx++] = 190; break;
                                case 230: bs[idx++] = 191; break;
                                case 260: bs[idx++] = 192; break;
                                case 302: bs[idx++] = 193; break;
                                case 256: bs[idx++] = 194; break;
                                case 262: bs[idx++] = 195; break;
                                case 196: bs[idx++] = 196; break;
                                case 197: bs[idx++] = 197; break;
                                case 280: bs[idx++] = 198; break;
                                case 274: bs[idx++] = 199; break;
                                case 268: bs[idx++] = 200; break;
                                case 201: bs[idx++] = 201; break;
                                case 377: bs[idx++] = 202; break;
                                case 278: bs[idx++] = 203; break;
                                case 290: bs[idx++] = 204; break;
                                case 310: bs[idx++] = 205; break;
                                case 298: bs[idx++] = 206; break;
                                case 315: bs[idx++] = 207; break;
                                case 352: bs[idx++] = 208; break;
                                case 323: bs[idx++] = 209; break;
                                case 325: bs[idx++] = 210; break;
                                case 211: bs[idx++] = 211; break;
                                case 332: bs[idx++] = 212; break;
                                case 213: bs[idx++] = 213; break;
                                case 214: bs[idx++] = 214; break;
                                case 215: bs[idx++] = 215; break;
                                case 370: bs[idx++] = 216; break;
                                case 321: bs[idx++] = 217; break;
                                case 346: bs[idx++] = 218; break;
                                case 362: bs[idx++] = 219; break;
                                case 220: bs[idx++] = 220; break;
                                case 379: bs[idx++] = 221; break;
                                case 381: bs[idx++] = 222; break;
                                case 223: bs[idx++] = 223; break;
                                case 261: bs[idx++] = 224; break;
                                case 303: bs[idx++] = 225; break;
                                case 257: bs[idx++] = 226; break;
                                case 263: bs[idx++] = 227; break;
                                case 228: bs[idx++] = 228; break;
                                case 229: bs[idx++] = 229; break;
                                case 281: bs[idx++] = 230; break;
                                case 275: bs[idx++] = 231; break;
                                case 269: bs[idx++] = 232; break;
                                case 233: bs[idx++] = 233; break;
                                case 378: bs[idx++] = 234; break;
                                case 279: bs[idx++] = 235; break;
                                case 291: bs[idx++] = 236; break;
                                case 311: bs[idx++] = 237; break;
                                case 299: bs[idx++] = 238; break;
                                case 316: bs[idx++] = 239; break;
                                case 353: bs[idx++] = 240; break;
                                case 324: bs[idx++] = 241; break;
                                case 326: bs[idx++] = 242; break;
                                case 243: bs[idx++] = 243; break;
                                case 333: bs[idx++] = 244; break;
                                case 245: bs[idx++] = 245; break;
                                case 246: bs[idx++] = 246; break;
                                case 247: bs[idx++] = 247; break;
                                case 371: bs[idx++] = 248; break;
                                case 322: bs[idx++] = 249; break;
                                case 347: bs[idx++] = 250; break;
                                case 363: bs[idx++] = 251; break;
                                case 252: bs[idx++] = 252; break;
                                case 380: bs[idx++] = 253; break;
                                case 382: bs[idx++] = 254; break;
                                case 729: bs[idx++] = 255; break;

                                default:
                                    bs[idx++] = (byte)'?';
                                    break;
                                //throw new System.Exception("Could not convert from unicode to CodePage E");
                            }
                            break;
                        case 'J':
                            switch ((int)s[i])
                            {
                                case 8364: bs[idx++] = 128; break;
                                case 8230: bs[idx++] = 133; break;
                                case 8224: bs[idx++] = 134; break;
                                case 8225: bs[idx++] = 135; break;
                                case 169: bs[idx++] = 136; break;
                                case 163: bs[idx++] = 137; break;
                                case 8249: bs[idx++] = 139; break;
                                case 8226: bs[idx++] = 149; break;
                                case 174: bs[idx++] = 152; break;
                                case 8482: bs[idx++] = 153; break;
                                case 127: bs[idx++] = 154; break;
                                case 8250: bs[idx++] = 155; break;
                                case 177: bs[idx++] = 156; break;
                                case 185: bs[idx++] = 157; break;
                                case 178: bs[idx++] = 158; break;
                                case 179: bs[idx++] = 159; break;
                                case 65377: bs[idx++] = 161; break;
                                case 65378: bs[idx++] = 162; break;
                                case 65379: bs[idx++] = 163; break;
                                case 65380: bs[idx++] = 164; break;
                                case 65381: bs[idx++] = 165; break;
                                case 65382: bs[idx++] = 166; break;
                                case 65383: bs[idx++] = 167; break;
                                case 65384: bs[idx++] = 168; break;
                                case 65385: bs[idx++] = 169; break;
                                case 65386: bs[idx++] = 170; break;
                                case 65387: bs[idx++] = 171; break;
                                case 65388: bs[idx++] = 172; break;
                                case 65389: bs[idx++] = 173; break;
                                case 65390: bs[idx++] = 174; break;
                                case 65391: bs[idx++] = 175; break;
                                case 65392: bs[idx++] = 176; break;
                                case 65393: bs[idx++] = 177; break;
                                case 65394: bs[idx++] = 178; break;
                                case 65395: bs[idx++] = 179; break;
                                case 65396: bs[idx++] = 180; break;
                                case 65397: bs[idx++] = 181; break;
                                case 65398: bs[idx++] = 182; break;
                                case 65399: bs[idx++] = 183; break;
                                case 65400: bs[idx++] = 184; break;
                                case 65401: bs[idx++] = 185; break;
                                case 65402: bs[idx++] = 186; break;
                                case 65403: bs[idx++] = 187; break;
                                case 65404: bs[idx++] = 188; break;
                                case 65405: bs[idx++] = 189; break;
                                case 65406: bs[idx++] = 190; break;
                                case 65407: bs[idx++] = 191; break;
                                case 65408: bs[idx++] = 192; break;
                                case 65409: bs[idx++] = 193; break;
                                case 65410: bs[idx++] = 194; break;
                                case 65411: bs[idx++] = 195; break;
                                case 65412: bs[idx++] = 196; break;
                                case 65413: bs[idx++] = 197; break;
                                case 65414: bs[idx++] = 198; break;
                                case 65415: bs[idx++] = 199; break;
                                case 65416: bs[idx++] = 200; break;
                                case 65417: bs[idx++] = 201; break;
                                case 65418: bs[idx++] = 202; break;
                                case 65419: bs[idx++] = 203; break;
                                case 65420: bs[idx++] = 204; break;
                                case 65421: bs[idx++] = 205; break;
                                case 65422: bs[idx++] = 206; break;
                                case 65423: bs[idx++] = 207; break;
                                case 65424: bs[idx++] = 208; break;
                                case 65425: bs[idx++] = 209; break;
                                case 65426: bs[idx++] = 210; break;
                                case 65427: bs[idx++] = 211; break;
                                case 65428: bs[idx++] = 212; break;
                                case 65429: bs[idx++] = 213; break;
                                case 65430: bs[idx++] = 214; break;
                                case 65431: bs[idx++] = 215; break;
                                case 65432: bs[idx++] = 216; break;
                                case 65433: bs[idx++] = 217; break;
                                case 65434: bs[idx++] = 218; break;
                                case 65435: bs[idx++] = 219; break;
                                case 65436: bs[idx++] = 220; break;
                                case 65437: bs[idx++] = 221; break;
                                case 65438: bs[idx++] = 222; break;
                                case 65439: bs[idx++] = 223; break;

                                default:
                                    bs[idx++] = (byte)'?';
                                    break;
                                //throw new System.Exception("Could not convert from unicode to CodePage E");
                            }
                            break;


                        default:
                            bs[idx++] = (byte)'?';
                            break;
                    }
                }
                else
                {
                    bs[idx++] = (byte)s[i];

                    if (specchar)
                    {
                        switch (s[i])
                        {
                            case 'E':
                            case 'C':
                            case 'L':
                            case 'G':
                            case 'T':
                            case 'B':
                            case 'J':
                                codepage = s[i];
                                break;

                            default: break;
                        }
                        specchar = false;
                    }
                }
            }
        }
    }
}
