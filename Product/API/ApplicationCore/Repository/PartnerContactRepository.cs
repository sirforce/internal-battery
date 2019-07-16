﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public class PartnerContactRepository : UpDiddyRepositoryBase<PartnerContact>, IPartnerContactRepository
    {
        UpDiddyDbContext _dbContext;
        IContactRepository _contactRepository;
        public PartnerContactRepository(UpDiddyDbContext dbContext, IContactRepository contactRepository) : base(dbContext)
        {
            _dbContext = dbContext;
            _contactRepository = contactRepository;
        }

        public async Task<IList<Partner>> GetPartnersAssociatedWithSubscriber(int SubscriberId)
        {
            IEnumerable<Contact> ieContact = await _contactRepository.GetByConditionAsync(c => c.SubscriberId == SubscriberId);
            Contact contact = ieContact.FirstOrDefault();

            // If user was not in contacts table prior to signing up, they're not associated with external partner
            // in our system, so return null.
            if (contact == null)
                return null;

            IList<Partner> Partners = new List<Partner>();
            IQueryable<PartnerContact> partnerContacts = await GetAllAsync();
            IQueryable<PartnerContact> iePartnerContacts = partnerContacts.Where(pc => pc.ContactId == contact.ContactId).Include<PartnerContact>("Partner");
            IList<PartnerContact> PartnerContacts = iePartnerContacts.ToList();
            foreach(PartnerContact partnerContact in PartnerContacts)
            {
                Partners.Add(partnerContact.Partner);
            }
            return Partners;
        }
    }
}
