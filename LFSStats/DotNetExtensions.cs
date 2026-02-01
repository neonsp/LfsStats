using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetExtensions
{
	/// <summary>
	/// A List&lt;Tuple&lt;T1, T2&gt;&gt; helper class.
	/// </summary>
	/// <typeparam name="T1">The type of the 1.</typeparam>
	/// <typeparam name="T2">The type of the 2.</typeparam>
	public class TupleList<T1, T2> : List<Tuple<T1, T2>>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TupleList&lt;T1, T2&gt;"/> class.
		/// </summary>
		public TupleList() : base() { }
		/// <summary>
		/// Initializes a new instance of the <see cref="TupleList&lt;T1, T2&gt;"/> class.
		/// </summary>
		/// <param name="collection">The collection to copy.</param>
		public TupleList(IEnumerable<Tuple<T1, T2>> collection) : base(collection) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="TupleList&lt;T1, T2&gt;"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public TupleList(int capacity) : base(capacity) { }

		/// <summary>
		/// Adds the specified items to the end of the System.Collections.Generic.List<Tuple<T1, T2>>.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="item2">The item2.</param>
		public void Add(T1 item, T2 item2)
		{
			Add(new Tuple<T1, T2>(item, item2));
		}
	}
}
