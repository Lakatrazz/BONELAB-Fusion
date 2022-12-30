using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Exceptions
{
    public class ExpectedClientException : Exception
    {
        public override string Message => "ExpectedClientException: Server has received a Message which expects a Client.";
    }
}

