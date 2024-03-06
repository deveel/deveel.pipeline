using Deveel.Pipelines;

var pipeline = new MathBuilder()
	.Use<AddHandler>()
	.Use<MultiplyHandler>()
	.Use<DivideHandler>()
	.Use<SubtractHandler>()
	.Build();

int? number;
Console.WriteLine("Please provide me a number: ");
while (true) {
	if (int.TryParse(Console.ReadLine(), out var value)) {
		number = value;
		break;
	} else {
		Console.WriteLine("Invalid number, please try again: ");
	}
}

var context = new MathContext {
	Number = number.Value
};

await pipeline.ExecuteAsync(context);

Console.WriteLine($"The result is: {context.Number}");


class MathBuilder : PipelineBuilder<MathContext> {
	public MathBuilder Use<THandler>() where THandler : class {
		AddStep<THandler>();
		return this;
	}

	public Pipeline<MathContext> Build() {
		return BuildPipeline(new PipelineBuildContext());
	}
}

class MathContext : PipelineExecutionContext {
	public int Number { get; set; }
}

abstract class MathHandler {
	protected abstract int Execute(int number, int value);

	public Task HandleAsync(MathContext context) {
		Console.Out.WriteLine($"Executing {GetType().Name} with {context.Number}");

		context.Number = Execute(context.Number, context.Number);
		return Task.CompletedTask;
	}
}

class AddHandler : MathHandler {
	protected override int Execute(int number, int value) {
		return number + value;
	}
}

class MultiplyHandler : MathHandler {
	protected override int Execute(int number, int value) {
		return number * value;
	}
}

class SubtractHandler : MathHandler {
	protected override int Execute(int number, int value) {
		return number - value;
	}
}

class DivideHandler : MathHandler {
	protected override int Execute(int number, int value) {
		return number / value;
	}
}