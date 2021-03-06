﻿import React from 'react';
import ReactTable from 'react-table';
import withFixedColumns from 'react-table-hoc-fixed-columns';
import DatePicker from 'react-datepicker';
import moment from 'moment';

const ReactTableFixedColumns = withFixedColumns(ReactTable);

class NotificationCountsTable extends React.Component {
    columns = [];
    constructor(props) {
        super(props);
        this.buildColumns(props);
        this.state = {
            data: props.data,
            loading: false,
            date: {
                ge: null,
                le: null
            }
        };
    }

    buildColumns(props) {
        this.columns = [
            {
                Header: "Notification Title",
                accessor: "notificationTitle"
            },
            {
                Header: "Published Date",
                accessor: "publishedDate",
                Cell: pd => <React.Fragment>{this.toLocalDate(pd.value)}</React.Fragment>
            },
            {
                Header: "Total Read",
                accessor: "readCount",
                headerClassName: 'text-right mr-2',
                className: 'text-right mr-2'
            }
        ];
    }

    toLocalDate(dateTime) {
        return moment(dateTime).local().format("MM-DD-YYYY hh:mm a");
        //return "1/1/2019";
    }

    handleStartDateChange(date) {
        this.setState({ date: { ge: date, le: this.state.date.le } }, () => this.reloadData());
    }

    handleEndDateChange(date) {
        this.setState({ date: { ge: this.state.date.ge, le: date } }, () => this.reloadData());
    }

    getQuery() {
        const query = { filter: {} };
        if (this.state.date.ge || this.state.date.le)
            query.filter.createDate = {};
        if (this.state.date.ge)
            query.filter.createDate.ge = this.state.date.ge;
        if (this.state.date.le)
            query.filter.createDate.le = this.state.date.le;
        return query;
    }

    reloadData() {
        this.setState({ loading: true }, () =>
            CareerCircleAPI.getNotificationCountReport(this.getQuery())
                .then((res) => {
                    this.setState({ data: res.data });
                })
                .catch(() => ToastService.error('An error occured while loading the report.'))
                .finally(() => {
                    this.setState({ loading: false });
                })
        );
    }

    render() {
        return (
            <div className="react-report-table">
                <div className="advanced-filters-wrapper p-2">
                    <div className="form-inline">
                        <div className="form-group mr-1">
                            <div className="d-block">
                                <DatePicker
                                    placeholderText="Start Date"
                                    selected={this.state.date.ge}
                                    onChange={(date) => this.handleStartDateChange(date)}
                                    showYearDropdown
                                    selectsStart
                                    startDate={this.state.date.ge}
                                    endDate={this.state.date.le}
                                    isClearable={true}
                                    maxDate={new Date()}
                                />
                            </div>
                        </div>
                        -
                    <div className="form-group ml-1">
                            <div className="d-block">
                                <DatePicker
                                    placeholderText="End Date"
                                    selected={this.state.date.le}
                                    onChange={(date) => this.handleEndDateChange(date)}
                                    showYearDropdown
                                    selectsEnd
                                    startDate={this.state.date.ge}
                                    endDate={this.state.date.le}
                                    isClearable={true}
                                    maxDate={new Date()}
                                />
                            </div>
                        </div>
                    </div>
                </div>
                <ReactTableFixedColumns
                    data={this.state.data}
                    columns={this.columns}
                    loading={this.state.loading}
                    showPagination={false}
                    className="-striped -highlight"
                    minRows={4}
                    noDataText="No Records"
                />
            </div>
        );
    }
}

export default NotificationCountsTable;