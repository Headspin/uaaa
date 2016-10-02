using System;

namespace Uaaa {
	/// <summary>
	/// Provides model business rule validation checking.
	/// </summary>
	public abstract class BusinessRule {
        /// <summary>
        /// Defines business rule error message.
        /// </summary>
		public readonly string Error = null;
        /// <summary>
        /// Creates new object instance.
        /// </summary>
        /// <param name="error"></param>
		protected BusinessRule (string error) {
			if (string.IsNullOrEmpty (error))
				throw new ArgumentNullException ("error");
			this.Error = error;
		}
        /// <summary>
        /// Validates the rule.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
		public abstract bool IsValid (object model);
	}

	/// <summary>
	/// Generic business rule implementation.
	/// </summary>
	public sealed class GenericRule<TModel>:BusinessRule {
		private readonly Predicate<TModel> criteria = null;
        /// <summary>
        /// Creates new object instance.
        /// </summary>
        /// <param name="criteria">Criteria for evaluation the rule validity.</param>
        /// <param name="error"></param>
		public GenericRule (Predicate<TModel> criteria, string error) : base (error) {
			if (criteria == null)
				throw new ArgumentNullException ("criteria");
			this.criteria = criteria;
		}
        ///<see cref="Uaaa.BusinessRule.IsValid(object)"/>
		public override bool IsValid (object model) {
			return criteria ((TModel)model);
		}
	}

}
