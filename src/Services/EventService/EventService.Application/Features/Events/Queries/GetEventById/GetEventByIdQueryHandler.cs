﻿using AutoMapper;
using EventService.Application.Constants;
using EventService.Application.Exceptions;
using EventService.Application.Models;
using EventService.Application.Persistence;
using EventService.Application.Utilities;
using EventService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.Features.Events.Queries.GetEventById
{
    public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventVm>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEventByIdQueryHandler> _logger;

        public GetEventByIdQueryHandler(IEventRepository eventRepository, IMapper mapper, ILogger<GetEventByIdQueryHandler> logger)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EventVm> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var eventDetails = await _eventRepository.GetEvent(request.EventId);
            if (eventDetails != null)
            {
                if (eventDetails.IsDeleted)
                {
                    throw new NotFoundException(string.Format(DomainErrors.EventNotFound, request.EventId));
                }

                if (eventDetails.CreatedBy != request.UserId)
                {
                    _logger.LogWarning(string.Format(DomainErrors.EventUserForbiddenAccess, request.UserId, request.EventId));
                    throw new ForbiddenAccessException();
                }

                ResetEventDatesToLocalTime(eventDetails);
            }

            return _mapper.Map<EventVm>(eventDetails);
        }

        private static void ResetEventDatesToLocalTime(Event? eventDetails)
        {
            if (eventDetails != null && eventDetails.Timezone == null) return;

            eventDetails.StartDate = eventDetails.StartDate.ToLocalDate(eventDetails.Timezone);
            eventDetails.EndDate = eventDetails.EndDate.ToLocalDate(eventDetails.Timezone);

            foreach (var notification in eventDetails.Notifications)
            {
                notification.NotificationDate = notification.NotificationDate.ToLocalDate(eventDetails.Timezone);
            }
        }
    }
}
