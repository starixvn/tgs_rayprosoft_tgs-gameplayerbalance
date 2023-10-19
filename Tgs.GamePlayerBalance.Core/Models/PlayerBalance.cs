using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerBalance(
    decimal Balance,
    decimal FreeBalance,
    decimal OutstandingBalance);
