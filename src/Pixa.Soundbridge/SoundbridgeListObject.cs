namespace Pixa.Soundbridge {
    public class SoundbridgeListObject : SoundbridgeObject {
        private int _index;
        private string _name;
        private Soundbridge _soundbridge;

        public SoundbridgeListObject(Soundbridge sb, int index, string name) : base(sb) {
            _index = index;
            _name = name;
            _soundbridge = sb;
        }

        public bool IsActive {
            get {
                return Soundbridge.ActiveList is object && Soundbridge.ActiveList.Contains(this);
            }
        }

        public int Index {
            get {
                return _index;
            }

            internal set {
                _index = value;
            }
        }

        public string Name {
            get {
                return _name;
            }
        }

        public Soundbridge Soundbridge {
            get {
                return _soundbridge;
            }
        }
    }
}