using System;

namespace Pixa.Soundbridge {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class IRCommandStringAttribute : Attribute {
        private string _commandString;

        public IRCommandStringAttribute(string commandString) {
            _commandString = commandString;
        }

        public string CommandString {
            get {
                return _commandString;
            }
        }
    }
}