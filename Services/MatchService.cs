﻿using OnlineBingoAPI.Contracts;
using OnlineBingoAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using OnlineBingoAPI.Models;
using OnlineBingoAPI.CustomException;
using System.Linq;
using OnlineBingoAPI.Extensions;

namespace OnlineBingoAPI.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        public MatchService(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public async Task AddNumber(Guid id, int number)
        {
            var match = await _matchRepository.Get(id);
            if (match == null)
                throw new NotFoundException("Match not found!");

            if (match.SelectedNumbers.Any(n => n == number))
                throw new BusinessRuleException("Number already selected");

            if (number < 1 || number > 99)
                throw new BusinessRuleException("The informed number is out of range.(1 - 99)");

            if (match.Status == Types.MatchStatus.Created)
                match.Status = Types.MatchStatus.InProgress;

            if (match.Status != Types.MatchStatus.InProgress)
                throw new BusinessRuleException("This match is not in progress");

            match.SelectedNumbers.Add(number);
            match.UpdateCards(number);

            var winner = match.GetWinner();
            if (winner != null)
                match.Status = Types.MatchStatus.Finished;

            await _matchRepository.Update(match);
        }

        public async Task<MatchReadContract> Create(MatchCreateContract newMatch)
        {
            var match = newMatch.Adapt<Match>();
            await _matchRepository.Add(match);
            return match.Adapt<MatchReadContract>();
        }

        public async Task Delete(Guid id)
        {
            var match = await _matchRepository.Get(id);
            if (match == null)
                throw new NotFoundException("Match not found!");
            await _matchRepository.Delete(id);
        }

        public async Task<MatchReadContract> Get(Guid id)
        {
            var match = await _matchRepository.Get(id);
            if (match == null)
                throw new NotFoundException("Match not found!");
            return match.Adapt<MatchReadContract>();
        }

        public async Task<IEnumerable<MatchReadContract>> GetAll()
        {
            var matches = await _matchRepository.GetAll();
            return matches.Select(m => m.Adapt<MatchReadContract>()).ToList();
        }

        public async Task Update(MatchUpdateContract match)
        {
            var exists = await _matchRepository.Get(match.Id);
            if (exists == null)
                throw new NotFoundException("Match not found");
            var updatedMatch = match.Adapt<Match>();
            updatedMatch.CreatedAt = exists.CreatedAt;
            await _matchRepository.Update(updatedMatch);
        }
    }
}
