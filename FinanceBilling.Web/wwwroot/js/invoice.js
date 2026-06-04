$(document).ready(function () {

    // AJAX live search on invoice list
    $('#invoiceSearch').on('keyup', function () {
        var searchTerm = $(this).val().toLowerCase();
        $('#invoiceTable tbody tr').filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(searchTerm) > -1);
        });
    });

    // AJAX status filter
    $('#statusFilter').on('change', function () {
        var selected = $(this).val();
        $('#invoiceTable tbody tr').each(function () {
            var statusCell = $(this).find('.invoice-status').text().trim();
            if (selected === '' || statusCell === selected) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    // AJAX payment form submission
    $('#ajaxPaymentForm').on('submit', function (e) {
        e.preventDefault();

        var form = $(this);
        var invoiceId = $('#ajaxInvoiceId').val();
        var amount = $('#ajaxAmount').val();
        var method = $('#ajaxPaymentMethod').val();

        if (!amount || parseFloat(amount) <= 0) {
            showAlert('danger', 'Please enter a valid amount.');
            return;
        }

        if (!method) {
            showAlert('danger', 'Please select a payment method.');
            return;
        }

        $.ajax({
            url: '/Invoice/ProcessPaymentAjax',
            type: 'POST',
            data: {
                invoiceId: invoiceId,
                amount: amount,
                paymentMethod: method,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showAlert('success', response.message);
                    setTimeout(function () {
                        location.reload();
                    }, 1500);
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function () {
                showAlert('danger', 'Something went wrong. Please try again.');
            }
        });
    });

    function showAlert(type, message) {
        var alertHtml = '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
            message +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
            '</div>';
        $('#ajaxAlertContainer').html(alertHtml);
    }

});