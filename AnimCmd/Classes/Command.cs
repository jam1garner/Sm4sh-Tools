﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sm4shCommand.Classes
{
    public unsafe class Command
    {
        public Command(Endianness _endian, CommandInfo info)
        {
            endian = _endian;
            _commandInfo = info;
        }
        public Command() { }

        public CommandInfo _commandInfo;
        public Endianness endian;


        public List<object> parameters = new List<object>();

        public virtual int CalcSize() { return 0x04 + (_commandInfo.ParamSpecifiers.Count * 4); }

        public override string ToString()
        {
            string Param = "";
            for (int i = 0; i < parameters.Count; i++)
            {

                if (_commandInfo.ParamSyntax.Count > 0)
                    Param += String.Format("{0}=", _commandInfo.ParamSyntax[i]);

                if (parameters[i] is int | parameters[i] is bint)
                    Param += String.Format("0x{0:X}{1}", parameters[i], i + 1 != parameters.Count ? ", " : "");
                if (parameters[i] is float | parameters[i] is bfloat)
                    Param += String.Format("{0}{1}", parameters[i], i + 1 != parameters.Count ? ", " : "");
                if (parameters[i] is decimal)
                    Param += String.Format("{0}{1}", parameters[i], i + 1 != parameters.Count ? ", " : "");

            }
            return String.Format("{0}({1})", _commandInfo.Name, Param);

        }
        public virtual byte[] GetArray()
        {
            byte[] tmp = new byte[CalcSize()];
            Util.SetWord(ref tmp, _commandInfo.Identifier, 0, endian);
            for (int i = 0; i < _commandInfo.ParamSpecifiers.Count; i++)
            {
                if (_commandInfo.ParamSpecifiers[i] == 0)
                    Util.SetWord(ref tmp, Convert.ToInt32(parameters[i]), (i + 1) * 4, endian);
                else if (_commandInfo.ParamSpecifiers[i] == 1)
                {
                    double HEX = Convert.ToDouble(parameters[i]);
                    float flt = (float)HEX;
                    byte[] bytes = BitConverter.GetBytes(flt);
                    int dec = BitConverter.ToInt32(bytes, 0);
                    string HexVal = dec.ToString("X");

                    Util.SetWord(ref tmp, Int32.Parse(HexVal, System.Globalization.NumberStyles.HexNumber), (i + 1) * 4, endian);
                }
            }
            return tmp;
        }
    }
    public unsafe class UnknownCommand : Command
    {
        public List<int> data = new List<int>();

        public override int CalcSize() { return data.Count * 4; }
        public override string ToString()
        {
            string s1 = "";
            for (int i = 0; i < data.Count; i++)
                s1 += String.Format("0x{0:X8}{1}", data[i], i + 1 != data.Count ? "\n" : "");
            return s1;
        }
        public override byte[] GetArray()
        {
            // ew this sux; you better fix this later, me.
            byte[] _data = new byte[data.Count * 4];
            for(int i=0; i<_data.Length; i+=4)
            {
                _data[i] = (byte)(data[i/4] >> 24);
                _data[i+1] = (byte)(data[i/4] >> 16);
                _data[i+2] = (byte)(data[i/4] >> 8);
                _data[i+3] = (byte)(data[i/4]);
            }
            return _data;
        }
    }
}