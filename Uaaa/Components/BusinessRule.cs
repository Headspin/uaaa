using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
	/// <summary>
	/// Provides model business rule validation checking.
	/// </summary>
	public abstract class BusinessRule {
		public readonly string Error = null;

		protected BusinessRule (string error) {
			if (string.IsNullOrEmpty (error))
				throw new ArgumentNullException ("error");
			this.Error = error;
		}

		public abstract bool IsValid (object model);
	}

	/// <summary>
	/// Generic business rule implementation.
	/// </summary>
	public sealed class GenericRule<TModel>:BusinessRule {
		private readonly Predicate<TModel> _criteria = null;

		public GenericRule (Predicate<TModel> criteria, string error) : base (error) {
			if (criteria == null)
				throw new ArgumentNullException ("criteria");
			_criteria = criteria;
		}

		public override bool IsValid (object model) {
			return _criteria ((TModel)model);
		}
	}

}
