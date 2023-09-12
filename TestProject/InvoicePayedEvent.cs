namespace TestProject
{
    /// <summary>
    /// Event that will be triggered if the invoice is payed
    /// </summary>
    /// <producer>Invoicing</producer>
    /// <producer>Billing</producer>
    /// <consumer>Accounts</consumer>
    /// <consumer>Notifications</consumer>
    /// <description>
    /// This event can either be triggered by the invoicing service, if the user has payed, or the billing service if the billing department has received cash money.
    /// Now also testing new lines
    ///     - which can contain tabs here and there, noo problem!
    /// </description>
    /// <process eventOrder="10">Order.Payment</process>
    /// <preceedingEvent><userInteraction>User Received Payment link</userInteraction></preceedingEvent>
    /// <preceedingEvent><userInteraction>Billing Accepted Payment</userInteraction></preceedingEvent>
    /// <succeedingEvent><see cref="OrderConfirmedEvent"/></succeedingEvent>
    /// <succeedingEvent><see cref="SendPaymentConfirmedEvent"/></succeedingEvent>
    public class InvoicePayedEvent
    {

        public string InvoiceNumber { get; set; }
        public double Ammount { get; set; }

    }
}