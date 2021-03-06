﻿namespace FluentArgs.Help
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentArgs.Description;
    using FluentArgs.Execution;
    using FluentArgs.Extensions;

    internal class HelpVisitor : IStepVisitor
    {
        private readonly IHelpPrinter helpPrinter;
        private Stack<(Name name, string description)> givenTexts = new Stack<(Name name, string description)>();

        public HelpVisitor(IHelpPrinter helpPrinter)
        {
            this.helpPrinter = helpPrinter;
        }

        public Task Visit(CallStep step)
        {
            return Task.CompletedTask;
        }

        public async Task Visit(FlagStep step)
        {
            await helpPrinter.WriteFlagInfos(step.Description.Name.Names, step.Description.Description, GetGivenHints()).ConfigureAwait(false);
            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public async Task Visit(GivenCommandStep step)
        {
            if (step.Branches[step.Branches.Count - 1].branch.Type == GivenCommandBranchType.Invalid)
            {
                // TODO: The command is required; print this somehow
                // or just throw an error if this case happens (at runtime)
            }

            await step.Branches.Select(async b =>
            {
                var giventextsAdded = false;

                switch (b.branch.Type)
                {
                    case GivenCommandBranchType.HasValue:
                        if ((b.branch.PossibleValues?.Length ?? 0) == 0)
                        {
                            givenTexts.Push((step.Name, "has a non-existing value"));
                        }
                        else if (b.branch.PossibleValues?.Length == 1)
                        {
                            givenTexts.Push((step.Name, $"is {b.branch.PossibleValues![0]}"));
                        }
                        else
                        {
                            givenTexts.Push((step.Name, $"is one of the following values: {string.Join(", ", b.branch.PossibleValues)}"));
                        }

                        giventextsAdded = true;

                        break;

                    case GivenCommandBranchType.Matches:
                        givenTexts.Push((step.Name, "matches a well-defined pattern"));
                        giventextsAdded = true;
                        break;

                    case GivenCommandBranchType.Ignore:
                        break;
                }

                if (b.then is FluentArgsDefinition argsBuilder)
                {
                    await argsBuilder.InitialStep.Accept(this).ConfigureAwait(false);
                }

                if (giventextsAdded)
                {
                    givenTexts.Pop();
                }
            }).Serialize().ConfigureAwait(false);

            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public async Task Visit(GivenFlagStep step)
        {
            givenTexts.Push((step.Description.Name, " is set"));

            if (step.ThenStep is FluentArgsDefinition argsBuilder)
            {
                await argsBuilder.InitialStep.Accept(this).ConfigureAwait(false);
            }

            givenTexts.Pop();

            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public async Task Visit(GivenParameterStep step)
        {
            if (step.Description.RequireExactValue)
            {
                givenTexts.Push((step.Description.Name, $" is {step.Description.RequiredValue}"));
            }
            else
            {
                givenTexts.Push((step.Description.Name, $" is set"));
            }

            if (step.ThenStep is FluentArgsDefinition argsBuilder)
            {
                await argsBuilder.InitialStep.Accept(this).ConfigureAwait(false);
            }

            givenTexts.Pop();

            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public async Task Visit(InitialStep step)
        {
            var applicationDescription = step.ParserSettings?.ApplicationDescription;
            if (applicationDescription != null)
            {
                await helpPrinter.WriteApplicationDescription(applicationDescription).ConfigureAwait(false);
            }

            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public Task Visit(InvalidStep step)
        {
            return Task.CompletedTask;
        }

        public async Task Visit(ListParameterStep step)
        {
            var listParameter = step.Description;
            await helpPrinter.WriteListParameterInfos(
                listParameter.Name.Names,
                listParameter.Description,
                listParameter.Type,
                !listParameter.IsRequired,
                listParameter.Separators,
                listParameter.HasDefaultValue,
                listParameter.DefaultValue,
                listParameter.Examples,
                GetGivenHints()).ConfigureAwait(false);
            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public async Task Visit(ParameterStep step)
        {
            var parameter = step.Description;
            await helpPrinter.WriteParameterInfos(
                parameter.Name.Names,
                parameter.Description,
                parameter.Type,
                !parameter.IsRequired,
                parameter.HasDefaultValue,
                parameter.DefaultValue,
                parameter.Examples ?? Array.Empty<string>(),
                GetGivenHints()).ConfigureAwait(false);
            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public async Task Visit(PositionalArgumentStep step)
        {
            var parameter = step.Description;
            await helpPrinter.WritePositionalArgumentInfos(
                parameter.Description,
                parameter.Type,
                !parameter.IsRequired,
                parameter.HasDefaultValue,
                parameter.DefaultValue,
                parameter.Examples ?? Array.Empty<string>(),
                GetGivenHints()).ConfigureAwait(false);
            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public async Task Visit(RemainingArgumentsStep step)
        {
            var description = step.Description;
            await helpPrinter.WriteRemainingArgumentsAreUsed(
                description.Description,
                description.Type,
                description.Examples ?? Array.Empty<string>(),
                GetGivenHints()).ConfigureAwait(false);
            await step.GetNextStep().Accept(this).ConfigureAwait(false);
        }

        public Task Visit(UntypedCallStep step)
        {
            return Task.CompletedTask;
        }

        private IReadOnlyCollection<(IReadOnlyCollection<string> aliases, string description)> GetGivenHints()
        {
            return givenTexts.Select(t => ((IReadOnlyCollection<string>)t.name.Names, t.description)).ToArray();
        }
    }
}
