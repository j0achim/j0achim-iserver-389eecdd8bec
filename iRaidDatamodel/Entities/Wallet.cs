using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Wallet : DomainLogic<Wallet>
    {
        public Wallet(User user, Community community, double Balance = 0)
        {
            this.UserId = user.Id;
            this.CommunityId = community.Id;
            this.Balance = Balance;
            this.Created = DateTime.UtcNow;
        }

        public long UserId { get; set; }
        public long CommunityId { get; set; }
        public double Balance { get; set; }
        public DateTime Created { get; set; }

        //
        // Methods
        //
        public List<Transaction> Transactions()
        {
            return Repository<Transaction>.FindBy(x => x.WalletId == this.Id).OrderBy(x => x.Time).ToList();
        }
    }

    [LoadInfo]
    public class Transaction : DomainLogic<Transaction>
    {
        public Transaction(User user, Wallet wallet, double Points)
        {
            this.WalletId = wallet.Id;
            this.Time = DateTime.UtcNow;
            this.OldBalance = wallet.Balance;
            this.NewBalance = wallet.Balance;
            this.Amount = Points;

            if(Points < 0)
            {
                this.NewBalance -= Math.Abs(Points);
            }
            else
            {
                this.NewBalance += Math.Abs(Points);
            }

            // Apply new balance.
            wallet.Balance = this.NewBalance;
            // Save Wallet
            wallet.Save(user);

            // Store Transaction.
            Save(user);
        }

        public long WalletId { get; set; }
        public double Amount { get; set; }
        public double OldBalance { get; set; }
        public double NewBalance { get; set; }
        public DateTime Time { get; set; }
    }
}
