﻿namespace FluentArgs.Playground
{
    using System;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Hello World!");

            args = new[] { "-m", "-n", "beni" };

            FluentArgsBuilder.New()
                //.Parameter<string>("-n", "--name").IsRequired()
                .Given.Flag("-m").Then(b => b
                    //   .Parameter<string>("-n").IsRequired()
                    .Call(() => //name =>
                    {
                        Console.WriteLine($"Hello !");
                    }))
                .Call(() =>
                {
                    Console.WriteLine("Brauche -m parameter");
                })
                .Parse(args);

            Console.ReadLine();
            return;

            FluentArgsBuilder.New()
                .ParameterList<int>("-n", "--numbers").IsRequired()
                .Parameter<string>("--name")
                    .WithDescription("")
                    .WithExamples("")
                    .IsRequired()
                .Call(name => numbers =>
                {

                })
                .Parse(args);

            FluentArgsBuilder.New()
                .Given.Parameter("-c", "--command").WithValue("copy").Then(b => b
                    .Parameter<string>("-i", "--input").IsRequired()
                    .Parameter<int>("-b", "--blocksize").IsOptional()
                    .Call(blockSize => input =>
                    {
                        return Task.CompletedTask;
                    }))
                .Parse(args);

            FluentArgsBuilder.New()

                /* Help etc. */
                .Given.Flag("-h", "--help").Then(b => b.Call(() =>
                {
                    Console.WriteLine("Show help"); // put this to an extension method
                }))

                /* general settings / arguments */
                .Parameter<string>("-k", "--apikey")
                    .WithDescription("the magic super expensive api key")
                    .WithExamples("ABC", "123")
                    .IsRequired()

                /* switch like command parameters */
                .Given.Command("-c", "--command")
                    .HasValue("copy").Then(b => b
                        .Parameter<string>("-i", "--input").IsRequired()
                        .Parameter<int>("-b", "--blocksize").IsOptionalWithDefault(-1)
                        .Call(b => i => key =>
                        {

                        }))
                    .HasValue("delete").Then(b => b
                        .Parameter<string>("-f", "--file").IsRequired()
                        .Call(file => key =>
                        {

                        }))
                    .ElseIgnore()

                .Given.Command("-x", "--execute")
                    .HasValue("now").Then(b => b.Call(key => Console.WriteLine("excute")))
                    .Matches<DateTimeOffset>(d => d != DateTimeOffset.Now, DateTimeOffset.Parse).Then(b => b.Call(Console.WriteLine))
                    .ElseIgnore()

                .Given.Command("-c")
                    .HasValue(1).Then(b => b
                        .Parameter<int>("u").IsRequired()
                        .Given.Command("-c2")
                            .HasValue(2).Then(b => b
                                .Parameter<double>("v").IsRequired()
                                .Call(v => u => key =>
                                {

                                }))
                            .ElseIsInvalid())
                    .ElseIgnore()

                .ParseAsync(args);
        }
    }
}
