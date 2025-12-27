using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Constants;

public class LoginFailReasons
{

    public const string ValidationError = "ValidationError";
    public const string InvalidCredentials = "InvalidCredentials";
    public const string UserDisabled = "UserDisabled";
    public const string EmailNotConfirmed = "EmailNotConfirmed";
    public const string LockedOut = "LockedOut";
}
