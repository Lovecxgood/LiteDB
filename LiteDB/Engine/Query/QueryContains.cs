using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    /// <summary>
    /// Contains query do not work with index, only full scan
    /// </summary>
    internal class QueryContains : Query
    {
        private BsonValue _value;

        public QueryContains(string field, BsonValue value)
            : base(field)
        {
            _value = value;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            return indexer
                .FindAll(index, Query.Ascending)
                .Where(x => x.Key.IsString && x.Key.AsString.Contains(_value));
        }

        internal override bool FilterDocument(BsonDocument doc)
        {
            return this.Expression.Execute(doc, false)
                .Where(x => x.IsString)
                .Any(x => x.AsString.Contains(_value));
        }

        public override BsonValue ToMongoQuery()
        {
            BsonDocument opt = new BsonDocument();
            opt.Add("$regex", _value);
            BsonDocument mq = new BsonDocument();
            mq.Add(this.Field, opt);
            return mq;
        }

        public override string ToString()
        {
            return string.Format("{0}({1} contains {2})",
                this.UseFilter ? "Filter" : this.UseIndex ? "Scan" : "",
                this.Expression?.ToString() ?? this.Field,
                _value);
        }
    }
}