﻿namespace FluentArgs.Execution
{
    using System.Threading.Tasks;
    using FluentArgs.Description;

    internal class FlagStep : Step
    {
        public FlagStep(Step previousStep, Flag flag)
            : base(previousStep)
        {
            Description = flag;
        }

        public Flag Description { get; }

        public override Task Accept(IStepVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override Task Execute(State state)
        {
            if (state.TryExtractFlag(Description.Name.Names, out _, out var newState))
            {
                state = newState.AddParameter(true);
            }
            else
            {
                state = state.AddParameter(false);
            }

            return GetNextStep().Execute(state);
        }
    }
}
