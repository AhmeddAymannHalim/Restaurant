namespace ResturantPL.ViewModels
{
    public class EndShiftViewModel
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public int hallSummaryNumber { get; set; }
        public decimal hallSummaryTotal { get; set; }
        public int deliverySummaryNumber { get; set; }
        public decimal deliverySummaryTotal { get; set; }
        public int deliveryOnlineSummaryNumber { get; set; }
        public decimal deliveryOnlineSummaryTotal { get; set; }
        public int takeAwaySummaryNumber { get; set; }
        public decimal takeAwaySummaryTotal { get; set; }


        public int pendingHallNumber { get; set; }
        public decimal pendingHallTotal { get; set; }
        public int pendingDeliveryNmuber { get; set; }
        public decimal pendingDeliveryTotal { get; set; }
        public int pendingDeliveryOnlineNmuber { get; set; }
        public decimal pendingDeliveryOnlineTotal { get; set; }
        public int totalPendingNumber { get; set; }
        public decimal totalPendingTotal { get; set; }


        public int lastPendingHallNumber { get; set; }
        public decimal lastPendingHallTotal { get; set; }
        public int lastPendingDeliveryNmuber { get; set; }
        public decimal lastPendingDeliveryTotal { get; set; }
        public int lastPendingDeliveryOnlineNmuber { get; set; }
        public decimal lastPendingDeliveryOnlineTotal { get; set; }
        public int lastTotalPendingNumber { get; set; }
        public decimal lastTotalPendingTotal { get; set; }


        public int totalSummeryNumber { get; set; }
        public decimal totalSummeryTotal { get; set; }
        public int selledLastPendingNumber { get; set; }
        public decimal selledLastPendingTotal { get; set; }
        public int TotalRemainForPendingNumber { get; set; }
        public decimal TotalRemainForPendingTotal { get; set; }
        public int totalMoneyNumber { get; set; }
        public decimal totalMoneyTotal { get; set; }
    }
}
