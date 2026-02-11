namespace Domain.Enums;
public enum UserRole
{
    Admin = 1,
    BranchManager = 2,
    FieldOfficer = 3,
    Accountant = 4
}
public enum MemberStatus
{
    Active = 1,
    Inactive = 2,
    Blacklisted = 3
}
public enum GroupStatus
{
    Forming = 1,
    Active = 2,
    Inactive = 3
}
public enum LoanStatus
{
    Pending = 1,
    Approved = 2,
    Disbursed = 3,
    Active = 4,
    Closed = 5,
    Rejected = 6,
    WrittenOff = 7
}
public enum LoanType
{
    General = 1,
    Agriculture = 2,
    Business = 3,
    Education = 4,
    Emergency = 5
}
public enum InterestType
{
    Flat = 1,
    ReducingBalance = 2,
    DecliningBalanceEMI = 3
}
public enum InstallmentStatus
{
    Pending = 1,
    Paid = 2,
    Partial = 3,
    Overdue = 4
}
public enum PaymentMethod
{
    Cash = 1,
    MobileBanking = 2,
    BankTransfer = 3
}
public enum TransactionType
{
    LoanDisbursement = 1,
    LoanRepayment = 2,
    SavingsDeposit = 3,
    SavingsWithdrawal = 4,
    FineCollection = 5,
    InterestIncome = 6
}
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Income = 4,
    Expense = 5
}
