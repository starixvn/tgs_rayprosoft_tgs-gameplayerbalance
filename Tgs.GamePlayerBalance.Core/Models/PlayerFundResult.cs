using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFundResult(
    StateCode StateCode,
    string Message,
    Guid FundTransactionId,
    Guid TransactionId,
    string ExternalTransactionId);
