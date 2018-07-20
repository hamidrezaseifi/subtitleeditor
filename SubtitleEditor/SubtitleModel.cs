using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SubtitleEditor
{
    public class SubtitleTime
    {
        private int _hour, _minute, _second, _millisecond;
        private bool _isValid;

        public SubtitleTime(string time)
        {
            _isValid = false;

            try
            {
                string[] parts = time.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    _millisecond = int.Parse(parts[1]);
                    parts = parts[0].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 2)
                    {
                        _hour = int.Parse(parts[0]);
                        _minute = int.Parse(parts[1]);
                        _second = int.Parse(parts[2]);
                        _isValid = true;
                    }
                }
            }
            catch
            {

            }

        }

        public override string ToString()
        {
            return _hour.ToString("00") + ":" + _minute.ToString("00") + ":" + _second.ToString("00") + "," + _millisecond.ToString("000");
        }

        public bool IsValid
        {
            get => _isValid; set => _isValid = value;
        }

        public void AddSeconds(int seconds)
        {

            DateTime dt = new DateTime(2000, 1, 1, _hour, _minute, _second);
            dt = dt.AddSeconds(seconds);
            _hour = dt.Hour;
            _minute = dt.Minute;
            _second = dt.Second;

        }

        private int TotalMillisecond
        {
            get
            {
                return _millisecond + _second * 1000 + _minute * 60 * 1000 + _hour * 60 * 60 * 1000;
            }

            set
            {
                int v = (int)(value / 1000);
                _millisecond = value - v * 1000;

                int m = (int)(v / 60);
                _second = v - m * 60;
                v = m;

                m = (int)(v / 60);
                _minute = v - m * 60;
                _hour = m;
                
            }
        }

        public void ChangeRate(float from, float to)
        {
            int t = TotalMillisecond;

            float ft = (t * to) / from;

            TotalMillisecond = (int)ft;
        }
    }

    public class SubtitleItem : INotifyPropertyChanged
    {
        private int _index;
        private SubtitleTime _from;
        private SubtitleTime _to;
        private IList<string> _textList;

        public event PropertyChangedEventHandler PropertyChanged;

        public SubtitleItem()
        {
            _from = new SubtitleTime("00:00:00,000");
            _to = new SubtitleTime("00:00:00,000");
            _textList = new List<string>();
            _index = -1;
        }

        public int Index
        {
            get => _index; set => _index = value;
        }

        public SubtitleTime From
        {
            get => _from; set => _from = value;
        }

        public SubtitleTime To
        {
            get => _to; set => _to = value;
        }

        public IList<string> TextList
        {
            get => _textList; set => _textList = value;
        }

        public string TextListString
        {
            get
            {
                string s = "";
                foreach (string line in _textList)
                {
                    s += line + "; \r\n";
                }
                return s;
            }
        }

        public void AddTextLine(string text)
        {
            _textList.Add(text);
        }

        public void SetTime(string text)
        {
            if (text.IndexOf("-->") > 0)
            {
                string[] parts = text.Split("-->".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    From = new SubtitleTime(parts[0]);
                    To = new SubtitleTime(parts[1]);

                }
            }
        }

        public void AddSeconds(int seconds)
        {
            From.AddSeconds(seconds);
            To.AddSeconds(seconds);

            OnPropertyChanged("From");
            OnPropertyChanged("To");
        }

        public void ChangeRate(float from, float to)
        {
            From.ChangeRate(from, to);
            To.ChangeRate(from, to);

            OnPropertyChanged("From");
            OnPropertyChanged("To");
        }

        public static bool isIndex(string text)
        {
            int i;
            return int.TryParse(text, out i);
        }

        public static bool isTime(string text)
        {
            if (text.IndexOf("-->") > 0)
            {
                string[] parts = text.Split("-->".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    SubtitleTime item1 = new SubtitleTime(parts[0]);
                    SubtitleTime item2 = new SubtitleTime(parts[1]);

                    return item1.IsValid && item2.IsValid;
                }
            }
            return false;
        }

        private void OnPropertyChanged(string property)
        {
            if (!ReferenceEquals(PropertyChanged, null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public IList<string> Lines
        {
            get
            {
                List<string> lines = new List<string>();
                lines.Add(Index.ToString());
                lines.Add(_from.ToString() + " --> " + _to.ToString());
                lines.AddRange(_textList);
                return lines;
            }
        }
    }

    public class SubtitleViewModel : INotifyPropertyChanged
    {
        private IList<SubtitleItem> _items;

        public event PropertyChangedEventHandler PropertyChanged;

        public SubtitleViewModel()
        {
            _items = new List<SubtitleItem>();
        }

        public bool Load(string file)
        {
            if (!File.Exists(file))
            {
                return false;
            }
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);
            _items = new List<SubtitleItem>();

            SubtitleItem item = new SubtitleItem();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.Length > 0)
                {

                    if (SubtitleItem.isIndex(line))
                    {
                        if (item.Index > -1)
                        {
                            _items.Add(item);
                            item = new SubtitleItem();
                        }

                        item.Index = int.Parse(line);
                    }
                    else
                    {
                        if (SubtitleItem.isTime(line))
                        {
                            item.SetTime(line);
                        }
                        else
                        {
                            item.AddTextLine(line);
                        }
                    }

                }


            }

            if (item.Index > -1)
            {
                _items.Add(item);
                item = new SubtitleItem();
            }
            ReOrder();

            OnPropertyChanged("Items");
            return true;
        }

        public void AddSeconds(int seconds)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].AddSeconds(seconds);
            }
            OnPropertyChanged("Items");
        }


        public void ChangeRate(float from, float to)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].ChangeRate(from, to);
            }
            OnPropertyChanged("Items");
        }

        public void RemoveAt(int index)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if(_items[i].Index == index)
                {
                    _items.RemoveAt(i);
                    break;
                }
            }
            ReOrder();
            OnPropertyChanged("Items");
        }

        private void ReOrder()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].Index = i + 1;
            }
            OnPropertyChanged("Items");
        }

        public IList<SubtitleItem> Items
        {
            get => _items; set => _items = value;
        }


        public bool Save(string file)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < _items.Count; i++)
            {
                lines.AddRange(_items[i].Lines);
                lines.Add("");
            }

            try
            {
                File.WriteAllLines(file, lines, Encoding.UTF8);
            }
            catch
            {

            }

            

            return true;
        }

        private void OnPropertyChanged(string property)
        {
            if(!ReferenceEquals(PropertyChanged,null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
