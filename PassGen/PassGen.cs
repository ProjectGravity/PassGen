﻿using System.Text;
using System;
using System.Collections.Generic;

namespace Gravity.PassGen
{
    /// <summary>
    /// Class for low level coding where you may need some hard coding
    /// </summary>
    public static class PassGenUtils
    {
        /// <summary>
        /// Generates an random string based on the "System.Random" algorithm
        /// </summary>
        /// <param name="Mask">All the possible character used in this string</param>
        /// <param name="length1">The length of the returned string</param>
        /// <returns></returns>
        public static string GenRandomPass(string Mask, int length1) //TODO: move to Generate instead of Utils
        {
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length1--)
            {
                char c = Mask[rnd.Next(0, Mask.Length)];
                //if ("+-&|!(){}[]^\"~*?:\\".Contains(c)) res.Append("\\\\");
                res.Append(c);
            }
            return res.ToString();
        }

        ///<summary>Generates an (PassGenMaskOptions) from lecagy parameters</summary>
        ///<param name="maskop">Outputs the (PassGenMaskOptions) for the parameters</param>
        /// <param name="parameters">Input of PassGenParameters where lecagy is enabled and legacy parameters are given</param>
        public static void LegacyParser(string legacyparameter, out PassGenMaskOptions maskop)
        {
            maskop = new PassGenMaskOptions();
            legacyparameter.Insert(legacyparameter.Length - 1, " ");
            char[] chars = legacyparameter.ToCharArray();
            bool Record = false;
            char lastchar = ' ';
            byte count = 0;
            int arraycount = 0;
            foreach (char ch in chars)
            {
                if (Record && lastchar != '}') maskop.Mask.Add(lastchar.ToString().ToLower() + lastchar.ToString().ToUpper());
                else Record = false; //if last character isn't the last in the string add the last to the masks in upper and lower case
                if (count != 0)      //Else stop adding the last character
                {
                    int ab = 0;
                    if(int.TryParse(ch.ToString(), out ab)) {
                        // if it doesn't work(e.g. NaN), it won't addup and it goes to else
                        arraycount += ab * (int)Math.Pow(10, count - 1);
                    }
                    else
                    {
                        count = 0;
                        maskop.Mask.Add(maskop.CustomMasks[arraycount]);
                        lastchar = ch;
                        continue;
                    }
                    count++;

                }
                switch (lastchar)
                {
                    case '?':
                        switch (ch)
                        {
                            case 'l': //lowercase
                                maskop.Mask.Add(maskop.DefaultMasks[0]); //abcdefghijklmnopqrstuvwxyz
                                break;
                            case 'u': //uppercase
                                maskop.Mask.Add(maskop.DefaultMasks[1]); //ABCDEFGHIJKLMNOPQRSTUVWXYZ
                                break;
                            case 'd': //decimal
                                maskop.Mask.Add(maskop.DefaultMasks[2]); //1234567890
                                break;
                            case 'c'://any Case
                                maskop.Mask.Add(maskop.DefaultMasks[0] + maskop.DefaultMasks[1]);
                                break;
                            case 't'://Three most common masks
                                maskop.Mask.Add(maskop.DefaultMasks[0] + maskop.DefaultMasks[1] + maskop.DefaultMasks[2]);
                                break;
                            case 's': //special
                                maskop.Mask.Add(maskop.DefaultMasks[3]); // !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~
                                break;
                            case 'a': //all
                                maskop.Mask.Add(maskop.DefaultMasks[0] + maskop.DefaultMasks[1] + maskop.DefaultMasks[2] + maskop.DefaultMasks[3]); //abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~
                                break;

                        }
                        break;
                    case '{':
                        Record = true;
                        break;
                    case '}':
                        Record = false;
                        break;
                    case '!': //Custom mask use; counting 0 as valid
                        count = 1;
                        break;
                    default:
                        
                        break;
                }
                lastchar = ch;
            }
        }
    }
    /// <summary>
    /// class for top level commands that don't involve a lot of hard coding
    /// </summary>
    public class PassGenGenerator
    {
        /// <summary>
        /// function for the masking of one string using either legacy parameters or normal ones
        /// </summary>
        /// <param name="s">string wich is going to be masked</param>
        /// <param name="parameters">parameter class containing all the options</param>
        /// <returns></returns>
        public string MaskString(string s, PassGenParameters parameters)
        {
            PassGenMaskOptions maskop;
            if (parameters.UseLegacyRules)
            {
               PassGenUtils.LegacyParser(parameters.LegacyRules, out maskop);
            }
            else
            {
                maskop = new PassGenMaskOptions();
                maskop.CustomMasks = new string[1];
                maskop.CustomMasks[0] = parameters.changeMask;
                for (int i = 0; i > parameters.additiveAmount; i++){ maskop.Mask.Add(parameters.additiveMask); }
                foreach (char c in s.ToCharArray())
                {
                    maskop.Mask.Add(maskop.CustomMasks[0]); //TODO: not good
                }
                for (int i = 0; i > parameters.additiveAmount; i++) { maskop.Mask.Add(parameters.additiveMask); }
            }
            ///Start Algorithm of Mask(input) Single time
            ulong a = MaskPossibility(maskop);
            return MaskFromSeed((uint)new Random().Next(1, (int)a), maskop);
        }

        /// <summary>
        /// Gets the amount of possiblities for this mask
        /// </summary>
        /// <param name="maskopt">Generator Mask</param>
        /// <returns>amount of possibilities</returns>
        public ulong MaskPossibility(PassGenMaskOptions maskopt)
        {
            ulong  apos = 1; //amount possible var
            foreach (string s in maskopt.Mask) apos = apos * (uint)s.Length;
            return apos;
        }

        /// <summary>
        /// Overload of Generates string from "seed" and "maskopt".
        /// </summary>
        /// <param name="seed">the seed as input</param>
        /// <param name="maskopt">maskoptions for the masking</param>
        /// <param name="apos">output of possible amount of seeds</param>
        /// <returns>string wich contains the now masked word</returns>
        public string MaskFromSeed(uint seed, PassGenMaskOptions maskopt, out ulong apos)
        {
            apos = 1; //amount possible var
            foreach(string s in maskopt.Mask) apos = apos * (uint)s.Length;
            return MaskFromSeed(seed, maskopt);
        }
        /// <summary>
        /// Generates string from "seed" and "maskopt"
        /// </summary>
        /// <param name="seed">the seed as input</param>
        /// <param name="maskopt">mask options for the masking</param>
        /// <returns>string wich contains the now masked word</returns>
        public string MaskFromSeed(ulong seed, PassGenMaskOptions maskopt)
        {
            char[] newString = new char[maskopt.Mask.Count];
            ulong next = seed;
            for(int c=0,i = maskopt.Mask.Count-1; i > -1; i--, c++)
            {
                int Length = maskopt.Mask.ToArray()[i].Length;
                    ulong thiss = next % (uint)Length;
                    next = next / (uint)Length;
                    newString[i] = maskopt.Mask.ToArray()[i].ToCharArray()[thiss];
           }
            return new string(newString);
        }      
    }

    public class PassGenParameters
    {
        public bool UseLegacyRules;
        public string LegacyRules;

        public byte additiveAmount; //max added characters
        public string additiveMask;
        public byte changeAmount; //max Simultanius changed letters, additive not counted
        public string changeMask;
    }
    public class PassGenMaskOptions
    {
        public List<string> Mask = new List<string>();
        public string[] CustomMasks; //max 100 in non-legacy mode
        public string[] DefaultMasks; //Init first
        public PassGenMaskOptions()
        {
            DefaultMasks = new string[6];
            DefaultMasks[0] = "abcdefghijklmnopqrstuvwxyz";
            DefaultMasks[1] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            DefaultMasks[2] = "0123456789";
            DefaultMasks[3] = " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
        }
    }

    public static class PassGenConst
    {
        public const string Mask_aZ09 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string Mask_aZ09_SPcompatible = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-+=";
        public const string Mask_aZ09_SPall = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!#$%&'()*+,-./:;<=>?@[]^_`{|}~";
    }
}
