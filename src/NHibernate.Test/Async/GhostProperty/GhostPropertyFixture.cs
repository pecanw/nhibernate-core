﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NHibernate.Cfg;
using NHibernate.Cfg.Loquacious;
using NHibernate.Tuple.Entity;
using NUnit.Framework;

namespace NHibernate.Test.GhostProperty
{
	using System.Threading.Tasks;
	[TestFixture]
	public class GhostPropertyFixtureAsync : TestCase
	{
		private string log;

		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override IList Mappings
		{
			get { return new[] { "GhostProperty.Mappings.hbm.xml" }; }
		}

		protected override void Configure(Cfg.Configuration configuration)
		{
			configuration.DataBaseIntegration(x=> x.LogFormattedSql = false);
		}

		protected override void OnSetUp()
		{
			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var wireTransfer = new WireTransfer
				{
					Id = 1
				};
				s.Persist(wireTransfer);
				s.Persist(new Order
				{
					Id = 1,
					Payment = wireTransfer
				});
				tx.Commit();
			}

		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				s.Delete("from Order");
				s.Delete("from Payment");
				tx.Commit();
			}
		}

		protected override DebugSessionFactory BuildSessionFactory()
		{
			using (var logSpy = new LogSpy(typeof(EntityMetamodel)))
			{
				var factory = base.BuildSessionFactory();
				log = logSpy.GetWholeLog();
				return factory;
			}
		}

		[Test]
		public async Task CanGetActualValueFromLazyManyToOneAsync()
		{
			using (ISession s = OpenSession())
			{
				var order = await (s.GetAsync<Order>(1));

				Assert.IsTrue(order.Payment is WireTransfer);
			}
		}

		[Test]
		public async Task WillNotLoadGhostPropertyByDefaultAsync()
		{
			using (ISession s = OpenSession())
			{
				var order = await (s.GetAsync<Order>(1));
				Assert.IsFalse(NHibernateUtil.IsPropertyInitialized(order, "Payment"));
			}
		}

		[Test]
		public async Task GhostPropertyMaintainIdentityMapAsync()
		{
			using (ISession s = OpenSession())
			{
				var order = await (s.GetAsync<Order>(1));

				Assert.AreSame(order.Payment, await (s.LoadAsync<Payment>(1)));
			}
		}

		[Test, Ignore("This shows an expected edge case")]
		public async Task GhostPropertyMaintainIdentityMapUsingGetAsync()
		{
			using (ISession s = OpenSession())
			{
				var payment = await (s.LoadAsync<Payment>(1));
				var order = await (s.GetAsync<Order>(1));

				Assert.AreSame(order.Payment, payment);
			}
		}

		[Test]
		public async Task WillLoadGhostAssociationOnAccessAsync()
		{
			// NH-2498
			using (ISession s = OpenSession())
			{
				Order order;
				using (var ls = new SqlLogSpy())
				{
					order = await (s.GetAsync<Order>(1));
					var logMessage = ls.GetWholeLog();
					Assert.That(logMessage, Does.Not.Contain("FROM Payment"));
				}
				Assert.That(NHibernateUtil.IsPropertyInitialized(order, "Payment"), Is.False);

				// trigger on-access lazy load 
				var x = order.Payment;
				Assert.That(NHibernateUtil.IsPropertyInitialized(order, "Payment"), Is.True);
			}
		}

		[Test]
		public async Task WhenGetThenLoadOnlyNoLazyPlainPropertiesAsync()
		{
			using (ISession s = OpenSession())
			{
				Order order;
				using (var ls = new SqlLogSpy())
				{
					order = await (s.GetAsync<Order>(1));
					var logMessage = ls.GetWholeLog();
					Assert.That(logMessage, Does.Not.Contain("ALazyProperty"));
					Assert.That(logMessage, Does.Contain("NoLazyProperty"));
				}
				Assert.That(NHibernateUtil.IsPropertyInitialized(order, "NoLazyProperty"), Is.True);
				Assert.That(NHibernateUtil.IsPropertyInitialized(order, "ALazyProperty"), Is.False);

				using (var ls = new SqlLogSpy())
				{
					var x = order.ALazyProperty;
					var logMessage = ls.GetWholeLog();
					Assert.That(logMessage, Does.Contain("ALazyProperty"));
				}
				Assert.That(NHibernateUtil.IsPropertyInitialized(order, "ALazyProperty"), Is.True);
			}
		} 
	}
}
