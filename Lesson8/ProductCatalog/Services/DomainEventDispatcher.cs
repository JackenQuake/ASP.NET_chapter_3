using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace DomainEventServices
{
	public class DomainEventDispatcher : IDomainEventDispatcher
	{
		private class Handler
		{
			public Type EventType { get; set; }
			public DomainEventHandler EventHandler { get; set; }
			public Handler Next { get; set; }
		}

		private Handler first;
		private readonly ILogger<DomainEventDispatcher> logger;

		public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
		{
			this.logger = logger;
			first = null;
			logger.LogInformation("DomainEventDispatcher создан.");
		}

		public void RegisterEventHandler(Type eventType, DomainEventHandler handler)
		{
			var newHandler = new Handler() { EventType = eventType, EventHandler = handler };
			// Вставка объекта в начало списка сводится к двум операциям: newHandler.Next = first и first = newHandler
			// Но в многопоточном окружении может быть проблема, если в промежутке между ними другой поток поменяет first
			// Следующий цикл должен сделать вставку аккуратно
			do
			{
				newHandler.Next = first;
			} while (Interlocked.CompareExchange(ref first, newHandler, newHandler.Next) != newHandler.Next);
			logger.LogInformation("DomainEventDispatcher: добавлен обработчик для события {EventType}.", eventType.ToString());
		}

		public void Raise(DomainEvent e)
		{
			logger.LogDebug("DomainEventDispatcher: raised event{@e}", e);
			for (var h = first; h != null; h = h.Next)
				if (h.EventType.IsInstanceOfType(e)) h.EventHandler(e);
		}
	}
}
