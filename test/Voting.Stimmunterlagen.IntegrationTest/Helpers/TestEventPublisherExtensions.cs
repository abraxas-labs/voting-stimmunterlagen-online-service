// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Google.Protobuf;
using Voting.Lib.Eventing.Testing.Mocks;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public static class TestEventPublisherExtensions
{
    /// <summary>
    /// This method publishes the provided event twice. This can be used to test for idempotency easily.
    /// </summary>
    /// <param name="publisher">The test publisher.</param>
    /// <param name="eventData">The event to publish twice.</param>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <returns>A task representing the async operation.</returns>
    public static Task PublishTwice<TEvent>(this TestEventPublisher publisher, TEvent eventData)
        where TEvent : IMessage<TEvent> => publisher.Publish(eventData, eventData);
}
