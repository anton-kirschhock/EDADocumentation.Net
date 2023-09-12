# EDA Documentation.Net

.NET CLI tool to help with documenting Event Driven Architectures, by using the XML documentation file of your event-libraries.

# What can it do?

It can transform all C# XML files to markdown files which then can be used to refer to how your event driven architecture works.
It can support multiple projects, as long you select them all when the cli prompts you to do so.

# Usage

## Preparing your documentation

To generate the proper documentation, you can use the default XML documentation tags in .Net and enrich them with additional tags, like so:

```cs
    /// <summary>
    /// Event that will be triggered if the invoice is payed
    /// </summary>
    /// <producer>Invoicing</producer>
    /// <producer>Billing</producer>
    /// <consumer>Accounts</consumer>
    /// <consumer>Notifications</consumer>
    /// <description>
    /// This event can either be triggered by the invoicing service, if the user has payed, or the billing service if the billing department has received cash money
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
```

| tag name        | description                                                                                                  |
| --------------- | ------------------------------------------------------------------------------------------------------------ |
| summary         | Quick summary of the event                                                                                   |
| producer        | (optional) All services which (can) produce the event. One event can be produced by multiple producers       |
| consumer        | (optional) All services which (can) consume the event. One event can be consumed by multiple consumers       |
| description     | longer description about the event, which can also contain Markdown                                          |
| process         | Indicates which processes this event is part of. One can order the event by using the attribute `eventOrder` |
| preceedingEvent | The direct preceeding event(s) of this event in the process event-chain                                      |
| succeedingEvent | The direct suceeding event(s) of this event in the process event-chain                                       |

### preceedingEvent & suceedingEvent tags

One can use either the `see` tag (with a valid cref), to link to an existing event type or `userInteraction` to indicate that the preceeding or suceeding event is a user interaction.

## Using the CLI

Next up start the cli using the following command:

```ps1
$> EDADocumentation.Net.exe parse [myWorkingDirectoryHere] [optional:myOutputDirectory]
```

Follow the instructions in the terminal to render the files.

# Contribute

Want to contribute or got great Ideas? Why not create a PR or start an discussion!

# TODO

- [x] Parse (multiple) XML files
- [x] Generate Services MD files
- [x] Generate Process MD files
- [x] Generate Event MD files
- [ ] Hyperlink support between files
- [ ] JSON output
- [ ] mdx and a react based visualizer tool
- [ ] Upsert support
- [ ] Pipeline support
- [ ] refactoring of code
