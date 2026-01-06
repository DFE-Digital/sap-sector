using Lucene.Net.Search;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services
{
    public class SearchService : ISearchService
    {
        private readonly ISearchRepository _searchRepository;
        private const int SearchMaxResults = 1000;

        public SearchService(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        public Task<IReadOnlyList<EstablishmentSearchResult>> SearchAsync(string query)
        {
            return _searchRepository.SearchAsync(query, SearchMaxResults);
        }

        public Establishment? SearchByNumber(string schoolNumber)
        {
            return _searchRepository.SearchByNumber(schoolNumber);
        }

        public Task<IReadOnlyList<EstablishmentSearchResult>> SuggestAsync(string queryPart)
        {
            return _searchRepository.SuggestAsync(queryPart);
        }
    }
}
