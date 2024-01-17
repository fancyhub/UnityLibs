/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    [System.Serializable]
    public struct Range32
    {
        public int _Min;
        public int _Max;
        public Range32(int min, int max)
        {
            _Min = min;
            _Max = max;
        }

        public int Min { get { return _Min; } set { _Min = value; } }
        public int Max { get { return _Max; } set { _Max = value; } }

        public int Range
        {
            get { return _Max - _Min; }
        }

        public int AbsRange
        {
            get { return _Max >= _Min ? (_Max - _Min) : (_Min - _Max); }
        }

        public int Lerp(float percent)
        {
            return _Min + (int)((_Max - _Min) * percent);
        }

        public int Random()
        {
            if (_Min == _Max)
                return _Min;
            return UnityEngine.Random.Range(_Min, _Max);            
        }

        public int Clamp(int v)
        {
            if (_Max > _Min)
            {
                if (v < _Min)
                    v = _Min;
                else if (v > _Max)
                    v = _Max;
            }
            else
            {
                if (v > _Max)
                    v = _Max;
                else if (v < _Min)
                    v = _Min;
            }
            return v;
        }

        public float GetClampPercent(int val)
        {
            float p = GetPercent(val);
            return UnityEngine.Mathf.Clamp(p, 0, 1);
        }

        public float GetPercent(int val)
        {
            int dur_dt = _Max - _Min;
            if (dur_dt == 0)
                return val >= _Max ? 1.0f : 0;

            int percent = (val - _Min) * 1000 / dur_dt;
            float ret = percent * 0.001f;
            return ret;
        }

        public static float GetPercent(int min, int max, int val)
        {
            Range32 r = new Range32(min, max);
            return r.GetPercent(val);
        }

        public static float GetClampPercent(int min, int max, int val)
        {
            Range32 r = new Range32(min, max);
            return r.GetClampPercent(val);
        }
    }

    [System.Serializable] 
    public struct RangeF32
    { 
        public float _Min; 
        public float _Max;

        public RangeF32(float min, float max)
        {
            _Min = min;
            _Max = max;
        }

        public float Lerp(float percent)
        {
            return _Min + (_Max - _Min) * percent;
        }

        public float Min { get { return _Min; } set { _Min = value; } }
        public float Max { get { return _Max; } set { _Max = value; } }

        public float Range
        {
            get { return _Max - _Min; }
        }

        public float Random()
        {           
            return UnityEngine.Random.Range(_Min, _Max);
        }

        public float AbsRange
        {
            get { return _Max >= _Min ? (_Max - _Min) : (_Min - _Max); }
        }

        public bool InRange(float v, bool exclude_min = false, bool exclude_max = false)
        {
            if (exclude_min && exclude_max)
                return _Min < v && v < _Max;
            else if (exclude_min && !exclude_max)
                return _Min < v && v <= _Max;
            else if (!exclude_min && exclude_max)
                return _Min <= v && v < _Max;
            else
                return _Min <= v && v <= _Max;
        }

        public float Clamp(float v)
        {
            if (_Max > _Min)
            {
                if (v < _Min)
                    v = _Min;
                else if (v > _Max)
                    v = _Max;
            }
            else
            {
                if (v > _Max)
                    v = _Max;
                else if (v < _Min)
                    v = _Min;
            }
            return v;
        }

        public float GetClampPercent(float val)
        {
            float p = GetPercent(val);
            //return System.Math.Clamp(p, 0.0f, 1.0f);
            return UnityEngine.Mathf.Clamp01(p);
        }

        public float GetPercent(float val)
        {
            float dur_dt = _Max - _Min;
            if (System.Math.Abs(dur_dt) < float.Epsilon)
                return val >= _Max ? 1.0f : 0;
            return (val - _Min) / dur_dt;
        }
    }

    [System.Serializable]
    public struct Range64
    {
        public long _Min;
        public long _Max;

        public Range64(long min, long max)
        {
            _Min = min;
            _Max = max;
        }

        public long Range
        {
            get { return _Max - _Min; }
        }

        public long AbsRange
        {
            get { return _Max >= _Min ? (_Max - _Min) : (_Min - _Max); }
        }

        public long Max { get { return _Max; } }
        public long Min { get { return _Min; } }

        public long Lerp(float percent)
        {
            return _Min + (long)((_Max - _Min) * percent);
        }

        public long Random()
        {
            float p = UnityEngine.Random.Range(0.0f, 1.0f);
            return Lerp(p);
        }

        public long Clamp(long v)
        {
            if (_Max > _Min)
            {
                if (v < _Min)
                    v = _Min;
                else if (v > _Max)
                    v = _Max;
            }
            else
            {
                if (v > _Max)
                    v = _Max;
                else if (v < _Min)
                    v = _Min;
            }
            return v;
        }

        public float GetClampPercent(long val)
        {
            float p = GetPercent(val);
            //return System.Math.Clamp(p, 0.0f, 1.0f);
            return UnityEngine.Mathf.Clamp01(p);
        }

        public float GetPercent(long val)
        {
            long dur_dt = _Max - _Min;
            if (dur_dt == 0)
                return val >= _Max ? 1.0f : 0;

            long percent = (val - _Min) * 1000 / dur_dt;
            float ret = percent * 0.001f;
            return ret;
        }

        public static float GetPercent(long min, long max, long val)
        {
            Range64 r = new Range64(min, max);
            return r.GetPercent(val);
        }

        public static float GetClampPercent(long min, long max, long val)
        {
            Range64 r = new Range64(min, max);
            return r.GetClampPercent(val);
        }
    }


    [System.Serializable]
    public struct RangeF64
    {
        public double _Min;
        public double _Max;

        public RangeF64(double min, double max)
        {
            _Min = min;
            _Max = max;
        }

        public double Lerp(float percent)
        {
            return _Min + (_Max - _Min) * percent;
        }

        public double Min { get { return _Min; } set { _Min = value; } }
        public double Max { get { return _Max; } set { _Max = value; } }

        public double Range
        {
            get { return _Max - _Min; }
        }

        public double Random()
        {
            return Lerp(UnityEngine.Random.Range(0, 1.0f));
        }

        public double AbsRange
        {
            get { return _Max >= _Min ? (_Max - _Min) : (_Min - _Max); }
        }

        public double Clamp(double v)
        {
            if (_Max > _Min)
            {
                if (v < _Min)
                    v = _Min;
                else if (v > _Max)
                    v = _Max;
            }
            else
            {
                if (v > _Max)
                    v = _Max;
                else if (v < _Min)
                    v = _Min;
            }
            return v;
        }

        public float GetClampPercent(double val)
        {
            float p = GetPercent(val);
            //return System.Math.Clamp(p, 0.0f, 1.0f);
            return UnityEngine.Mathf.Clamp01(p);
        }

        public float GetPercent(double val)
        {
            double dur_dt = _Max - _Min;
            if (System.Math.Abs(dur_dt) < float.Epsilon)
                return val >= _Max ? 1.0f : 0;
            return (float)((val - _Min) / dur_dt);
        }
    }

}
