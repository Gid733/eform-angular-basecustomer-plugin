﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using Customers.Pn.Helpers;
using Customers.Pn.Infrastructure.Data;
using Customers.Pn.Infrastructure.Models.Fields;
using eFormApi.BasePn.Infrastructure.Models.API;
using NLog;

namespace Customers.Pn.Controllers
{
    [Authorize]
    public class FieldsPnController : ApiController
    {
        private readonly Logger _logger;
        private readonly CustomersPnDbContext _dbContext;

        public FieldsPnController()
        {
            _dbContext = CustomersPnDbContext.Create();
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [Route("api/fields-pn")]
        public OperationDataResult<FieldsPnUpdateModel> GetFields()
        {
            try
            {
                var fields = _dbContext.CustomerFields
                    .Include("Field")
                    .Select(x => new FieldPnUpdateModel()
                    {
                        FieldStatus = x.FieldStatus,
                        Id = x.FieldId,
                        Name = x.Field.Name,
                    }).ToList();

                var result = new FieldsPnUpdateModel()
                {
                    Fields = fields,
                };
                return new OperationDataResult<FieldsPnUpdateModel>(true, result);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.Error(e);
                return new OperationDataResult<FieldsPnUpdateModel>(false,
                    CustomersPnLocaleHelper.GetString("ErrorWhileObtainingFieldsInfo"));
            }
        }

        [HttpPut]
        [Route("api/fields-pn")]
        public OperationResult UpdateFields(FieldsPnUpdateModel fieldsModel)
        {
            try
            {
                var list = fieldsModel.Fields.Select(s => s.Id).ToList();
                var fields = _dbContext.CustomerFields
                    .Where(x => list.Contains(x.FieldId))
                    .ToList();

                foreach (var field in fields)
                {
                    var fieldModel = fieldsModel.Fields.FirstOrDefault(x => x.Id == field.FieldId);
                    if (fieldModel != null)
                    {
                        field.FieldStatus = fieldModel.FieldStatus;
                    }
                }
                _dbContext.SaveChanges();
                return new OperationResult(true);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.Error(e);
                return new OperationResult(false, "ErrorWhileUpdatingFields");
            }
        }
    }
}
