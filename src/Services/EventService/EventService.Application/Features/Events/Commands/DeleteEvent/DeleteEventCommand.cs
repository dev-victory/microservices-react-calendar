﻿using MediatR;

namespace EventService.Application.Features.Events.Commands.DeleteEvent
{
    public class DeleteEventCommand : IRequest
    {
        public Guid EventId { get; set; }
        public string UserId { get; set; }
    }
}
