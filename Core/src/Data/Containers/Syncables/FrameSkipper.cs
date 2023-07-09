using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data {
    public class FrameSkipper {
        private readonly int _targetCounter;
        private int _currentCounter = 0;

        public FrameSkipper(int targetCounter) {
            _targetCounter = targetCounter;
        }

        public bool IsMatchingFrame() {
            _currentCounter++;

            if (_currentCounter >= _targetCounter) {
                _currentCounter = 0;
                return true;
            }
            else {
                return false;
            }
        }
    }
}
